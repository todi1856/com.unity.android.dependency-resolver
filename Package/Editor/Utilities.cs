using System.IO;
using System.Text;
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
                return $"The following templates have to be disabled for dependency resolver to work correctly:\n{errors}";

            return string.Empty;
        }
    }
}
