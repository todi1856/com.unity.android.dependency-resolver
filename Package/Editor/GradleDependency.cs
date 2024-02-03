using System.Collections.Generic;
using Unity.Android.Gradle;

namespace Unity.Android.DependencyResolver
{
    class GradleDependency
    {
        HashSet<string> m_SourceLocations;

        public string Value { get; private set; }

        public GradleDependency(string value)
        {
            Value = value;
            m_SourceLocations = new HashSet<string>();
        }

        public void AddSourceLocation(string path)
        {
            m_SourceLocations.Add(path);
        }
    }
}