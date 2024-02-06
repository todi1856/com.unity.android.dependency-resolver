using System;
using System.IO;
using System.Linq;
using UnityEditor;
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
        private Toggle m_EnableDependencyResolver;
        private Button m_RefreshDependencies;
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
                    l.style.marginLeft = 5.0f;
                    l.style.marginTop = 5.0f;
                    l.AddManipulator(new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) =>
                    {
                        evt.menu.AppendAction("Copy Dependency",  (x) => {
                            GUIUtility.systemCopyBuffer = ((GradleDependencyLabel)x.userData).Target.Value;
                        }, DropdownMenuAction.AlwaysEnabled, l);

                        evt.menu.AppendAction("Open Dependency in Web Browser", (x) => {
                            var label = ((GradleDependencyLabel)x.userData).Target;
                            var repo = label.Repository;
                            if (repo != null)
                            {
                                var url = $"{repo.Value}/web/index.html#{label.Value}";
                                Application.OpenURL(url);
                            }
                        }, DropdownMenuAction.AlwaysEnabled, l);

                        evt.menu.AppendAction("Copy Repository", (x) => {
                            var repo = ((GradleDependencyLabel)x.userData).Target.Repository;
                            if (repo != null)
                                GUIUtility.systemCopyBuffer = repo.Value;
                        }, DropdownMenuAction.AlwaysEnabled, l);

                    }));
                    return l;
                };

            multiColumnListView.selectionChanged += objects =>
            {
                var label = (GradleDependency)objects.FirstOrDefault();
                if (label != null)
                {
                    m_Locations.columns[0].title = $"<b>Source location for '{label.Value}'</b>";
                    m_Locations.itemsSource = label.SourceLocations.ToArray();
                    m_Locations.RefreshItems();
                }
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

                l.AddManipulator(new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) =>
                {
                    evt.menu.AppendAction("Show In Explorer", (x) => {
                        EditorUtility.RevealInFinder(((Label)l).text);
                    }, DropdownMenuAction.AlwaysEnabled, l);

                }));

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

            var validationResult = Utilities.CheckIfTemplatesAreDisabled();
            if (!string.IsNullOrEmpty(validationResult))
                container.Add(new HelpBox(validationResult, HelpBoxMessageType.Warning));

            validationResult = Utilities.CheckIfGoogleExternalDependencyManagerPresent();
            if (!string.IsNullOrEmpty(validationResult))
            {
                var box = new Box();

                box.Add(new HelpBox(validationResult, HelpBoxMessageType.Warning));
                var b = new Button(() =>
                {

                    Utilities.EDM4UDisableAutoResolving();
                    Utilities.EDM4UDeleteResolveLibraries();
                    EditorUtility.RequestScriptReload();
                })
                {
                    text = "Attempt to disable External Dependency Manager",
                };
                b.style.flexShrink = 1;
                b.style.flexGrow = 0;
                box.Add(b);
                container.Add(box);
            }

            container.Add(new Label("<size=15px><b>General</b><br>") { enableRichText = true });
            m_EnableDependencyResolver = new Toggle("Enable Dependency Resolver:")
            {
                value = ResolverSettings.Enabled
            };
            m_EnableDependencyResolver.RegisterCallback<ChangeEvent<bool>>((e) =>
            {
                SetDependencyResolverStatus(e.newValue);
                if (ResolverSettings.Enabled)
                    CollectDependencies();
            });
            container.Add(m_EnableDependencyResolver);

            container.Add(new Label("<br><size=15px><b>Diagnostics</b><br>") { enableRichText = true });

            m_RefreshDependencies = new Button(CollectDependencies)
            {
                text = "Refresh Dependencies"
            };
            container.Add(m_RefreshDependencies);
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

            SetDependencyResolverStatus(ResolverSettings.Enabled);
        }

        private void SetDependencyResolverStatus(bool enabled)
        {
            ResolverSettings.Enabled = enabled;
            m_Dependencies.SetEnabled(enabled);
            m_Locations.SetEnabled(enabled);
            m_RefreshDependencies.SetEnabled(enabled);
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
