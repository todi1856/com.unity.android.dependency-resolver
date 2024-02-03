/*
using System;
using System.Linq;
using Unity.Android.Gradle;
using UnityEditor.Android;


namespace Unity.Android.DependencyResolver
{
    public class GradleProjectModifier : AndroidProjectFilesModifier
    {
        [Serializable]
        public class Data
        {
            public string[] Repositories;
            public string[] Dependencies;
        }

        public override AndroidProjectFilesModifierContext Setup()
        {
            var collector = new Collector();
            var info = collector.CollectDependencies();

            var context = new AndroidProjectFilesModifierContext();
            context.SetData(nameof(Data), new Data()
            {
                Repositories = info.Repositories.Select(r => r.Value).ToArray(),
                Dependencies = info.Dependencies.Select(d => d.Value).ToArray()
            });
            return context;
        }

        public override void OnModifyAndroidProjectFiles(AndroidProjectFiles projectFiles)
        {
            var data = projectFiles.GetData<Data>(nameof(Data));
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
*/
// TODO: Add warning if specific templates are enabled