using System.Collections.Generic;

namespace Unity.Android.DependencyResolver
{
    class ResolverResult
    {
        private Dictionary<string, GradleRepository> m_Repositories;
        private Dictionary<string, GradleDependency> m_Dependencies;

        public IReadOnlyCollection<GradleRepository> Repositories => m_Repositories.Values;
        public IReadOnlyCollection<GradleDependency> Dependencies => m_Dependencies.Values;

        public ResolverResult()
        {
            m_Repositories = new Dictionary<string, GradleRepository>();
            m_Dependencies = new Dictionary<string, GradleDependency>();
        }

        public GradleRepository AddRepository(string name)
        {
            if (m_Repositories.TryGetValue(name, out var value))
                return value;

            var repository = new GradleRepository(name);
            m_Repositories.Add(name, repository);
            return repository;
        }

        public GradleDependency AddDependency(string name)
        {
            if (m_Dependencies.TryGetValue(name, out var value))
                return value;

            var dependency = new GradleDependency(name);
            m_Dependencies.Add(name, dependency);
            return dependency;
        }
    }
}