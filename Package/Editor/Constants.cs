using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Unity.Android.DependencyResolver
{
    class Constants
    {
        internal static readonly string LocalRepository = "localRepository";
        internal const string Assets = "Assets";
        internal const string Packages = "Packages";
        internal const string UrlFile = "${rootProject.projectDir.path}/";
        internal const string XmlExtension = ".xml";
    }
}