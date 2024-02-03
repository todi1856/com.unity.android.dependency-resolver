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

            if (data.Enabled)
            {
                var collector = new Collector();
                var info = collector.CollectDependencies();
                data.Repositories = info.Repositories.Select(r => r.Value).ToArray();
                data.Dependencies = info.Dependencies.Select(d => d.Value).ToArray();
            }
            var context = new AndroidProjectFilesModifierContext();
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