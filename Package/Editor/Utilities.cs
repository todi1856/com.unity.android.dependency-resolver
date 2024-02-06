using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.Android.DependencyResolver
{
    class Utilities
    {
        internal static string CheckIfTemplatesAreDisabled()
        {
            var androidPlugins = "Assets/Plugins/Android";
            var templatesToCheck = new[]
            {
                "gradleTemplate.properties",
                "mainTemplate.gradle",
                "settingsTemplate.gradle"
            };

            var errors = new StringBuilder();
            foreach (var template in templatesToCheck)
            {
                var relativePath = Path.Combine(androidPlugins, template).Replace("\\", "/");
                var path = Path.Combine(Application.dataPath, "..", relativePath);
                if (!File.Exists(path))
                    continue;

                errors.AppendLine($"- '{relativePath}'");
            }

            if (errors.Length > 0)
                return $"The following templates have to be disabled for the dependency resolver to work correctly:\n{errors}";

            return string.Empty;
        }

        internal static string CheckIfGoogleExternalDependencyManagerPresent()
        {
            Assembly jarAssembly = null;
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var a in loadedAssemblies)
            {
                if (a.FullName.Contains("Google.JarResolver"))
                {
                    jarAssembly = a;
                    break;
                }
            }
            if (jarAssembly == null)
                return string.Empty;

            var location = jarAssembly.Location;
            var root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            if (location.Contains(root))
                location = location.Substring(root.Length + 1);
            return $"Detected Google External Dependency Manager at\n'{location}'\nIt can conflict with Dependency Resolver, consider disabling or removing it.";
        }

        internal static void EDM4UDeleteResolveLibraries()
        {
            var menuItem = "Assets/External Dependency Manager/Android Resolver/Delete Resolved Libraries";
            EditorApplication.ExecuteMenuItem(menuItem);
            Debug.Log($"Executing '{menuItem}'");
        }

        internal static void EDM4UDisableAutoResolving()
        {
            var settingsPath = Path.Combine("ProjectSettings/GvhProjectSettings.xml");
            Debug.Log($"Patching '{settingsPath}'");
            if (!File.Exists(settingsPath))
                return;
            try
            {
                var settings = XDocument.Load(settingsPath);
                var root = settings.Root;
                foreach (var s in root.Elements("projectSetting"))
                {
                    var name = s.Attribute("name").Value;
                    if (name == "GooglePlayServices.AutoResolutionDisabledWarning" ||
                        name == "GooglePlayServices.AutoResolveOnBuild")
                        s.Attribute("value").Value = "False";
                }
                settings.Save(settingsPath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }
        }
    }
}
