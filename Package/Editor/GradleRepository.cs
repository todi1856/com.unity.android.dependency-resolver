using System.Collections.Generic;

namespace Unity.Android.DependencyResolver
{
    class GradleRepository
    {
        HashSet<GradleDependency> m_Dependencies;
        HashSet<string> m_SourceLocations;

        public string Value { get; private set; }

        public IReadOnlyCollection<GradleDependency> Dependencies => m_Dependencies;

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
    }
}