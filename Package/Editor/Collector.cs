using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System.Xml.Linq;

namespace Unity.Android.DependencyResolver
{
    class Collector
    {
        public ResolverResult CollectDependencies()
        {
            var paths =  AssetDatabase.GetAllAssetPaths()
                .Where(p => Path.GetExtension(p).Equals(".xml", System.StringComparison.InvariantCultureIgnoreCase))
                .ToArray();

            var result = new ResolverResult();
            foreach (var path in paths)
            {
                var doc = XDocument.Load(path);

                var root = doc.Element("dependencies");
                if (root == null)
                    continue;

                foreach (var packages in root.Elements("androidPackages"))
                {
                    foreach (var package in packages.Elements("androidPackage"))
                    {
                        var dependencyValueAttr = package.Attribute("spec");
                        if (dependencyValueAttr == null)
                            continue;

                        var dependency = result.AddDependency(dependencyValueAttr.Value);
                        dependency.AddSourceLocation(path);

                        var repositories = package.Element("repositories");
                        if (repositories == null)
                            continue;

                        foreach (var repository in repositories.Elements("repository"))
                        {
                            var addedRepository = result.AddRepository(repository.Value);
                            addedRepository.AddDependency(dependency);
                            addedRepository.AddSourceLocation(path);
                        }
                    }
                }
            }

            return result;
        }
    }
}