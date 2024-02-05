using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Unity.Android.DependencyResolver
{
    class ResolverProjectSettingsProvider : SettingsProvider
    {
        internal static readonly string SettingsRelativePath = "Android/Dependency Resolver";
        internal static readonly string SettingsPath = $"Project/{SettingsRelativePath}";
        class Styles
        {
            public static GUIContent symbolPaths = new GUIContent("Symbol Paths", "Configure symbol paths, used for resolving stack traces.");
        }

        private ResolverResult m_Result;


        public ResolverProjectSettingsProvider(string path, SettingsScope scope)
            : base(path, scope)
        {
        }


        public override void OnGUI(string searchContext)
        {
            var validationResult = Utilities.CheckIfTemplatesAreDisabled();
            if (!string.IsNullOrEmpty(validationResult))
                EditorGUILayout.HelpBox(validationResult, MessageType.Warning);

            EditorGUILayout.LabelField(Styles.symbolPaths, EditorStyles.boldLabel);
            if (GUILayout.Button("Collect"))
            {
                var c = new Collector();

                m_Result = c.CollectDependencies();
            }


            ResolverSettings.Enabled = GUILayout.Toggle(ResolverSettings.Enabled, "Enabled:");

            if (m_Result == null)
                return;
            GUILayout.Label($"Dependencies [{m_Result.Dependencies.Count}] Repositories [{m_Result.Repositories.Count}]:");
            foreach (var repo in m_Result.Repositories)
            {
                GUILayout.Label($"{repo.Value} Local: {repo.IsLocal}");
                if (repo.IsLocal)
                {
                    foreach (var file in repo.EnumerateLocalFiles())
                        GUILayout.Label($" - {file}");
                }
                GUILayout.Label("Dependencies:");
                foreach (var dep in repo.Dependencies)
                    GUILayout.Label($" - {dep.Value}");
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
