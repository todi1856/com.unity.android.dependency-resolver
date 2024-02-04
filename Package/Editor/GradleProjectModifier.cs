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
        class ModifiedFile
        {
            public string Path;
            public string Contents;
        }

        [Serializable]
        class Data
        {
            public bool Enabled;
            public string[] Repositories;
            public string[] Dependencies;
            public ModifiedFile[] ModifiedFiles;
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
                data.Repositories = info.Repositories.Select(r => r.ResolveRepositoryPath).ToArray();
                data.Dependencies = info.Dependencies.Select(d => d.Value).ToArray();

                var extraFiles = new List<ModifiedFile>();
                foreach (var repo in info.Repositories)
                {
                    if (!repo.IsLocal)
                        continue;

                    var root = Application.dataPath;
                    foreach (var file in repo.EnumerateLocalFiles())
                    {
                        var dst = Path.Combine(Constants.LocalRepository, file.Substring(root.Length + 1));

                        // Replicate hack from Google External Dependency Manager
                        // For legacy reasons, maven packages placed in Unity project have .aar files with .srcaar extension insted
                        // So Unity would ignore those files (otherwise they would end up as plugins in gradle project)
                        // When copying this packages from Unity project to gradle project
                        // We need to restore .aar extension both for file and in .pom file
                        var extension = Path.GetExtension(file);

                        // Patch pom file
                        if (extension.Equals(POMExtension))
                        {
                            var contents = File.ReadAllText(file);
                            contents = contents.Replace("srcaar", "aar");
                            extraFiles.Add(new ModifiedFile()
                            {
                                Path = dst,
                                Contents = contents
                            });
                            context.Outputs.AddFileWithContents(dst);
                            continue;
                        }

                        if (extension.Equals(SrcAARExtension))
                            dst = dst.Substring(0, dst.Length - SrcAARExtension.Length) + ".aar";
                        context.AddFileToCopy(file, dst);
                    }
                }

                data.ModifiedFiles = extraFiles.ToArray();
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

            foreach (var file in data.ModifiedFiles)
            {
                projectFiles.SetFileContents(file.Path, file.Contents);
            }

        }
    }
}
// TODO: Add warning if specific templates are enabled