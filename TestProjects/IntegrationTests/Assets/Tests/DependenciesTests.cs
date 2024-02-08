using UnityEditor;
using NUnit.Framework;
using System.Text;
using System.IO;
using System.Security.Policy;

namespace Tests
{
    [TestFixture]
    public class DependenciesTests : EditorTestBase
    {
        private const string DummyDependency = "com.unity.sdk:dummy:7.2.5";
        private const string DummyRepository = "https://maven.test.com/";

        private readonly string DummyDependencyInGradle = $"implementation '{DummyDependency}'";
        private readonly string DummyRepositoryInGradle = $"url \"{DummyRepository}\"";

        private string CreateDummyXmlContents(string dependency, string repository)
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
            CreateAssetWithTextContent(kAssetsTempPath, "dependencies.xml", CreateDummyXmlContents(DummyDependency, DummyRepository));
            AssetDatabase.Refresh();

            var location = BuildProject("DepsInAssets");
            var ulGradle = GetUnityLibraryBuildGradle(location);
            var sGradle = GetSettingsGradle(location);

            StringAssert.Contains(DummyDependencyInGradle, ulGradle);
            StringAssert.Contains(DummyRepositoryInGradle, sGradle);

            DeleteAsset(kAssetsTempPath);

            location = BuildProject("DepsInAssets");
            ulGradle = GetUnityLibraryBuildGradle(location);
            sGradle = GetSettingsGradle(location);

            StringAssert.DoesNotContain(DummyDependencyInGradle, ulGradle);
            StringAssert.DoesNotContain(DummyRepositoryInGradle, sGradle);
        }

        [Test]
        public void CanInjectDependenciesFromXmlInPackage()
        {
            CreateAssetWithTextContent(kPackageTempPath, "dependencies.xml", CreateDummyXmlContents(DummyDependency, DummyRepository));
            AssetDatabase.Refresh();

            /*
            var location = BuildProject("DepsInAssets");
            var ulGradle = GetUnityLibraryBuildGradle(location);
            var sGradle = GetSettingsGradle(location);

            StringAssert.Contains(DummyDependencyInGradle, ulGradle);
            StringAssert.Contains(DummyRepositoryInGradle, sGradle);

            DeleteAsset(kAssetsTempPath);

            location = BuildProject("DepsInAssets");
            ulGradle = GetUnityLibraryBuildGradle(location);
            sGradle = GetSettingsGradle(location);

            StringAssert.DoesNotContain(DummyDependencyInGradle, ulGradle);
            StringAssert.DoesNotContain(DummyRepositoryInGradle, sGradle);
            */
        }
    }
}
