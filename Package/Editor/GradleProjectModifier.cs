using System;
using System.Collections.Generic;
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
        internal readonly string POMExtension = ".pom";

        [Serializable]
        class Data
        {
            public bool Enabled;
            public string[] Repositories;
            public string[] Dependencies;
            public string[] PomFiles;
        }

        private static string CalculateDestinationPath(string sourcePath)
        {
            var root = Application.dataPath;
            return Path.Combine(Constants.LocalRepository, sourcePath.Substring(root.Length + 1)); ;
        }

        public override AndroidProjectFilesModifierContext Setup()
        {
            var data = new Data()
            {
                Enabled = ResolverSettings.Enabled
            };

            if (data.Enabled)
            {
                var validationResult = Utilities.CheckIfTemplatesAreDisabled();
                if (!string.IsNullOrEmpty(validationResult))
                {
                    data.Enabled = false;
                    throw new Exception($"{validationResult}\nSee also 'Project Settings/{ResolverProjectSettingsProvider.SettingsRelativePath}'.\n");
                }
            }

            var context = new AndroidProjectFilesModifierContext();
            if (data.Enabled)
            {
                var collector = new Collector();
                var info = collector.CollectDependencies();
                data.Repositories = info.Repositories.Select(r => r.ResolveRepositoryPath).ToArray();
                data.Dependencies = info.Dependencies.Select(d => d.Value).ToArray();

                var pomFiles = new List<string>();
                foreach (var repo in info.Repositories)
                {
                    if (!repo.IsLocal)
                        continue;

                    foreach (var file in repo.EnumerateLocalFiles())
                    {
                        var dst = CalculateDestinationPath(file);

                        // Replicate hack from Google External Dependency Manager
                        // For legacy reasons, maven packages placed in Unity project have .aar files with .srcaar extension insted
                        // So Unity would ignore those files (otherwise they would end up as plugins in gradle project)
                        // When copying this packages from Unity project to gradle project
                        // We need to restore .aar extension both for file and in .pom file
                        var extension = Path.GetExtension(file);

                        if (extension.Equals(POMExtension))
                        {
                            // .pom files require patching, do it in OnModifyAndroidProjectFiles, so it could be done in incremental way.
                            pomFiles.Add(file);
                            context.Outputs.AddFileWithContents(CalculateDestinationPath(file));
                        }
                        else
                        {

                            if (extension.Equals(SrcAARExtension))
                                dst = dst.Substring(0, dst.Length - SrcAARExtension.Length) + ".aar";
                            context.AddFileToCopy(file, dst);
                        }
                    }
                }

                data.PomFiles = pomFiles.ToArray();
                context.Dependencies.DependencyFiles = pomFiles.ToArray();
                
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

            foreach (var file in data.PomFiles)
            {
                var contents = File.ReadAllText(file);
                contents = contents.Replace("srcaar", "aar");
                projectFiles.SetFileContents(CalculateDestinationPath(file), contents);
            }

        }
    }
}