using System;
using System.IO;
using System.Linq;
using Unity.Android.Gradle;
using UnityEditor.Android;
using UnityEngine;

namespace Unity.Android.DependencyResolver
{
    public class GradleProjectModifier : AndroidProjectFilesModifier
    {
        internal readonly string SrcAARExtension = ".srcaar";

        [Serializable]
        public class Data
        {
            public bool Enabled;
            public string[] Repositories;
            public string[] Dependencies;
        }

        public override AndroidProjectFilesModifierContext Setup()
        {
            var data = new Data()
            {
                Enabled = ResolverSettings.Enabled
            };

            var context = new AndroidProjectFilesModifierContext();
            if (data.Enabled)
            {
                var collector = new Collector();
                var info = collector.CollectDependencies();
                data.Repositories = info.Repositories.Select(r => r.Value).ToArray();
                data.Dependencies = info.Dependencies.Select(d => d.Value).ToArray();

                foreach (var repo in info.Repositories)
                {
                    if (!repo.IsLocal)
                        continue;

                    var root = Application.dataPath;
                    foreach (var file in repo.EnumerateLocalFiles())
                    {
                        var dst = Path.Combine("Local", file.Substring(root.Length + 1));

                        // Replicate hack from Google External Dependency Manager
                        var extension = Path.GetExtension(file);
                        if (extension.Equals(SrcAARExtension))
                            dst = dst.Substring(0, dst.Length - SrcAARExtension.Length) + ".aar";
                        context.AddFileToCopy(file, dst);
                    }
                }
            }
            
            context.SetData(nameof(Data), data);
            return context;
        }

        public override void OnModifyAndroidProjectFiles(AndroidProjectFiles projectFiles)
        {
            var data = projectFiles.GetData<Data>(nameof(Data));
            if (!data.Enabled)
                return;
            foreach (var dependency in data.Dependencies)
                projectFiles.UnityLibraryBuildGradle.Dependencies.AddDependencyImplementationByName(dependency);

            var gradleRepositories = projectFiles.GradleSettings.DependencyResolutionManagement.Repositories;
            foreach (var repository in data.Repositories)
            {
                var block = new Block(Repositories.Maven);
                gradleRepositories.AddElement(block);
                block.AddElement(new Element($"url \"{repository}\""));
                // TALK TO RYTIS
            }
            // TODO: mavenLocal ?

        }
    }
}
// TODO: Add warning if specific templates are enabled