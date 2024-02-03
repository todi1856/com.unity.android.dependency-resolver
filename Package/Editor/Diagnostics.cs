using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Android.DependencyResolver
{
    public class Diagnostics : EditorWindow
    {
        private ResolverResult m_Result;
        [MenuItem("Examples/My Editor Window")]
        public static void ShowExample()
        {
            var wnd = GetWindow<Diagnostics>();
            wnd.titleContent = new GUIContent("MyEditorWindow");
        }
        /*
        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // VisualElements objects can contain other VisualElement following a tree hierarchy
            Label label = new Label("Hello World!");
            root.Add(label);

            // Create button
            Button button = new Button();
            button.name = "button";
            button.text = "Button";
            root.Add(button);

            // Create toggle
            Toggle toggle = new Toggle();
            toggle.name = "toggle";
            toggle.label = "Toggle";
            root.Add(toggle);
        }
        */
        public void OnGUI()
        {
            if (GUILayout.Button("Collect"))
            {
                var c = new Collector();

                m_Result = c.CollectDependencies();
            }

            if (m_Result == null)
                return;
            GUILayout.Label($"Dependencies [{m_Result.Dependencies.Count}] Repositories [{m_Result.Repositories.Count}]:");
            foreach (var repo in m_Result.Repositories)
            {
                GUILayout.Label(repo.Value);
                foreach (var dep in repo.Dependencies)
                    GUILayout.Label($" - {dep.Value}");
            }
        }
    }
}