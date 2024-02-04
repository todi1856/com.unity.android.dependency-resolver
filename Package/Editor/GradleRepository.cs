using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Unity.Android.DependencyResolver
{
    class GradleRepository
    {
        HashSet<GradleDependency> m_Dependencies;
        HashSet<string> m_SourceLocations;

        public string Value { get; private set; }

        public string ResolveRepositoryPath
        {
            get
            {
                if (IsLocal)
                {
                    var prefix = new[] { Constants.Assets, Constants.Packages };
                    foreach (var p in prefix)
                    {
                        if (Value.StartsWith(p, System.StringComparison.InvariantCultureIgnoreCase))
                        {
                            var relativePath = Path.Combine(Constants.LocalRepository, Value.Substring(p.Length + 1)).Replace("\\", "/");
                            return $"{Constants.UrlFile}{relativePath}";
                        }
                    }
                    return Value;
                }
                return Value;
            }
        }

        public IReadOnlyCollection<GradleDependency> Dependencies => m_Dependencies;

        public bool IsLocal => Value.StartsWith("Assets", System.StringComparison.InvariantCultureIgnoreCase) ||
            Value.StartsWith("Packages", System.StringComparison.InvariantCultureIgnoreCase);

        public GradleRepository(string value)
        {
            Value = value;
            m_Dependencies = new HashSet<GradleDependency>();
            m_SourceLocations = new HashSet<string>();
        }

        public void AddDependency(GradleDependency dependency)
        {
            m_Dependencies.Add(dependency);
        }

        public void AddSourceLocation(string path)
        {
            m_SourceLocations.Add(path);
        }

        public IEnumerable<string> EnumerateLocalFiles()
        {
            if (!IsLocal)
                throw new System.Exception("Can only enumerate files from a local repository");

            // TODO: files from packages
            // TODO: Add check function if repository exists
            var path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", Value));
            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            var ignoredExtesions = new []{ ".meta", ".xml" };
            foreach (var file in files)
            {
                var extension = Path.GetExtension(file);
                if (ignoredExtesions.Contains(extension))
                    continue;
                yield return file;
            }
        }
    }
}