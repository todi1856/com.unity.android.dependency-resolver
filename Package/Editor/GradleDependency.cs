using System.Collections.Generic;
using UnityEngine;

namespace Unity.Android.DependencyResolver
{
    class GradleDependency
    {
        HashSet<string> m_SourceLocations;
        GradleRepository m_Repository;
        public string Value { get; private set; }

        public GradleRepository Repository
        {
            set
            {
                if (Repository != null && !Repository.Value.Equals(value.Value))
                {
                    Debug.LogWarning($"The repository for package '{Value}' was already set ('{Repository.Value}'), the new repository is '{value.Value}'");
                    return;
                }

                m_Repository = value;
            }
            get => m_Repository;
        }

        public IReadOnlyCollection<string> SourceLocations => m_SourceLocations;

        public GradleDependency(string value)
        {
            Value = value;
            m_Repository = null;
            m_SourceLocations = new HashSet<string>();
        }

        public void AddSourceLocation(string path)
        {
            m_SourceLocations.Add(path);
        }
    }
}