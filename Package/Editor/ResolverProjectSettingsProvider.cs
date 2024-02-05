using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Android.DependencyResolver
{
    class GradleDependencyLabel : Label
    {
        public GradleDependency Target { set; get; }
    }

    class ResolverProjectSettingsProvider : SettingsProvider
    {
        class MyAllPostprocessor : AssetPostprocessor
        {
            static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
            {
                if (importedAssets == null)
                    return;
                foreach (var asset in importedAssets)
                {
                    // Only care about .xml files
                    if (Path.GetExtension(asset) == Constants.XmlExtension)
                    {
                        m_AssetUpdated = DateTime.Now;
                        break;
                    }
                }
            }
        }

        internal static readonly string SettingsRelativePath = "Android/Dependency Resolver";
        internal static readonly string SettingsPath = $"Project/{SettingsRelativePath}";

        private ResolverResult m_Result;
        private MultiColumnListView m_Dependencies;
        private MultiColumnListView m_Locations;
        private static DateTime m_AssetUpdated;
        private static DateTime m_DependenciesCollected;

        public ResolverProjectSettingsProvider(string path, SettingsScope scope)
            : base(path, scope)
        {
            activateHandler = CreateGUI;
        }

        private MultiColumnListView CreateDependencyView(Func<GradleDependency, int, string> getValue)
        {
            var multiColumnListView = new MultiColumnListView();
            multiColumnListView.style.flexGrow = 1.0f;
            multiColumnListView.style.flexShrink = 1.0f;
            multiColumnListView.style.marginTop = 5.0f;
            multiColumnListView.columns.Add(new Column { title = "<b>Dependency Name</b>", minWidth = 300 });
            multiColumnListView.columns.Add(new Column { title = "<b>Repository</b>", stretchable = true });
            multiColumnListView.showAlternatingRowBackgrounds = AlternatingRowBackground.All;
            multiColumnListView.itemsSource = new string[]
            {
            };

            multiColumnListView.showBorder = true;

            foreach (var c in multiColumnListView.columns)
                c.makeCell = () =>
                {
                    var l = new GradleDependencyLabel();
                    l.RegisterCallback<ClickEvent>((e) =>
                    {
                        if (l.Target == null)
                            return;
                        m_Locations.columns[0].title = $"<b>Source location for '{l.Target.Value}'</b>";
                        m_Locations.itemsSource = l.Target.SourceLocations.ToArray();
                        m_Locations.RefreshItems();
                    });

                    l.style.marginLeft = 5.0f;
                    l.style.marginTop = 5.0f;
                    return l;
                };

            Action<VisualElement, int> BindCell(int column)
            {
                return (element, index) =>
                {
                    var l = (GradleDependencyLabel)element;
                    l.Target = ((GradleDependency[])multiColumnListView.itemsSource)[index];
                    l.text = getValue(l.Target, column);
                };
            }
            // Note: Cannot do it in a loop due lambda
            multiColumnListView.columns[0].bindCell = BindCell(0);
            multiColumnListView.columns[1].bindCell = BindCell(1);

            multiColumnListView.RefreshItems();
            return multiColumnListView;
        }

        private MultiColumnListView CreateLocationView()
        {
            var multiColumnListView = new MultiColumnListView();
            multiColumnListView.style.flexGrow = 1.0f;
            multiColumnListView.style.flexShrink = 1.0f;
            multiColumnListView.style.marginTop = 5.0f;
            multiColumnListView.columns.Add(new Column { title = "<b>Source location</b>", stretchable = true });
            multiColumnListView.itemsSource = new string[]
            {
            };

            multiColumnListView.showBorder = true;
            multiColumnListView.showAlternatingRowBackgrounds = AlternatingRowBackground.All;

            multiColumnListView.columns[0].makeCell = () => 
            {
                var l = new Label();
                l.RegisterCallback<ClickEvent>((e) =>
                {
                    var label = (Label)e.target;
                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(label.text));
                });

                l.style.marginLeft = 5.0f;
                l.style.marginTop = 5.0f;
                return l;
            };
            multiColumnListView.columns[0].bindCell = (element, index) => ((Label)element).text = ((string[]) multiColumnListView.itemsSource)[index];

            multiColumnListView.RefreshItems();
            return multiColumnListView;
        }

        private void CreateGUI(string searchContext, VisualElement rootVisualElement)
        {
            var container = new VisualElement();
            container.style.marginLeft = 5.0f;
            container.style.marginRight = 5.0f;
            rootVisualElement.Add(container);

            container.Add(new Label("<size=20px><b>Dependency Resolver</b><br>") { enableRichText = true });
            container.Add(new IMGUIContainer(GUI));

            m_Dependencies = CreateDependencyView((GradleDependency dependency, int column) =>
            {
                return column switch
                {
                    0 => dependency.Value,
                    1 => dependency.Repository == null ? "<Not set>" : dependency.Repository.Value,
                    _ => "Not Implemented",
                };
            });
            container.Add(m_Dependencies);

            m_Locations = CreateLocationView();
            container.Add(m_Locations);

            if (ResolverSettings.Enabled)
            {
                m_AssetUpdated = DateTime.Now;
                CollectDependencies();
            }

            m_Dependencies.SetEnabled(ResolverSettings.Enabled);
            m_Locations.SetEnabled(ResolverSettings.Enabled);
        }

        private void UpdateData()
        {
            m_Dependencies.itemsSource = m_Result.Dependencies.ToArray();
            m_Dependencies.RefreshItems();
        }

        private void CollectDependencies()
        {
            m_Result = new Collector().CollectDependencies();
            UpdateData();

            m_DependenciesCollected = DateTime.Now;
        }
      
        public void GUI()
        {
            var validationResult = Utilities.CheckIfTemplatesAreDisabled();
            if (!string.IsNullOrEmpty(validationResult))
                EditorGUILayout.HelpBox(validationResult, MessageType.Warning);

            EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(GUILayout.Width(Screen.width));
            EditorGUILayout.LabelField("Enable Dependency Resolver:");
            EditorGUI.BeginChangeCheck();
            ResolverSettings.Enabled = GUILayout.Toggle(ResolverSettings.Enabled, GUIContent.none);
            if (EditorGUI.EndChangeCheck())
            {
                m_Dependencies.SetEnabled(ResolverSettings.Enabled);
                m_Locations.SetEnabled(ResolverSettings.Enabled);
                if (ResolverSettings.Enabled)
                    CollectDependencies();
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(!ResolverSettings.Enabled);
            EditorGUILayout.LabelField("Diagnostics", EditorStyles.boldLabel);
            if (GUILayout.Button("Refresh Dependencies", GUILayout.ExpandWidth(false)))
            {
                CollectDependencies();
            }

            EditorGUILayout.Space();
            EditorGUI.EndDisabledGroup();

            // Automatically collect dependencies if assets were updated
            if (ResolverSettings.Enabled && m_AssetUpdated > m_DependenciesCollected)
            {
                CollectDependencies();
            }
        }
        
        [SettingsProvider]
        public static SettingsProvider CreateAndroidLogcatProjectSettingsProvider()
        {
            var provider = new ResolverProjectSettingsProvider(SettingsPath, SettingsScope.Project);
            return provider;
        }
    }
}
