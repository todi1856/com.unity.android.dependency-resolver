using UnityEditor;
using NUnit.Framework;
using System.Text;
using System.IO;

namespace Tests
{
    [TestFixture]
    public class DependenciesTests : EditorTestBase
    {
        private string CreateDummyXml(string dependency, string repository)
        {
            var contents = new StringBuilder();

            contents.AppendLine(
@$"<dependencies>
  <unityversion>7.2.5.2</unityversion>
  <androidPackages>
    <androidPackage spec=""{dependency}"">");

            if (!string.IsNullOrEmpty(repository))
                contents.AppendLine(
@$"      <repositories>
        <repository>{repository}</repository>
      </repositories>");

            contents.AppendLine(
@$"  </androidPackage>
  </androidPackages>
</dependencies>");

            return contents.ToString();
        }

        private string BuildProject(string projectName)
        {
            var location = GenerateBuildLocation(projectName);
            Utilities.BuildProject(location, true);
            return location;
        }

        private string GetUnityLibraryBuildGradle(string projectPath)
        {
            return File.ReadAllText(Path.Combine(projectPath, "unityLibrary/build.gradle"));
        }

        private string GetSettingsGradle(string projectPath)
        {
            return File.ReadAllText(Path.Combine(projectPath, "settings.gradle"));
        }

        [Test]
        public void CanInjectDependenciesFromXmlInAssets()
        {
            CreateAssetWithTextContent(kAssetsTempPath, "dependencies.xml", CreateDummyXml("com.unity.sdk:dummy:7.2.5", "https://maven.test.com/"));
            AssetDatabase.Refresh();

            var location = BuildProject("DepsInAssets");
            var ulGradle = GetUnityLibraryBuildGradle(location);
            var sGradle = GetSettingsGradle(location);

            StringAssert.Contains("implementation 'com.unity.sdk:dummy:7.2.5'", ulGradle);
            StringAssert.Contains("url \"https://maven.test.com/\"", sGradle);

            DeleteAsset(kAssetsTempPath);

            location = BuildProject("DepsInAssets");
            ulGradle = GetUnityLibraryBuildGradle(location);
            sGradle = GetSettingsGradle(location);

            StringAssert.DoesNotContain("implementation 'com.unity.sdk:dummy:7.2.5'", ulGradle);
            StringAssert.DoesNotContain("url \"https://maven.test.com/\"", sGradle);
        }
    }
}
