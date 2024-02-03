using System.Collections.Generic;
using Unity.Android.Gradle;

namespace Unity.Android.DependencyResolver
{
    class GradleDependency
    {
        public string Value { get; private set; }

        public GradleDependency(string value)
        {
            Value = value;
        }
    }
}