using System.Collections.Generic;

namespace Unity.Android.DependencyResolver
{
    class GradleRepository
    {
        HashSet<GradleDependency> m_Dependencies;

        public string Value { get; private set; }

        public IReadOnlyCollection<GradleDependency> Dependencies => m_Dependencies;

        public GradleRepository(string value)
        {
            Value = value;
            m_Dependencies = new HashSet<GradleDependency>();
        }

        public void AddDependency(GradleDependency dependency)
        {
            m_Dependencies.Add(dependency);
        }
    }
}