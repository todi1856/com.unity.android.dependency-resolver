using UnityEditor;
using NUnit.Framework;
using System.Text;
using System.IO;
using System.Security.Policy;
using Unity.Android.Gradle.Manifest;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

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

            var location = BuildProject("DepsInPackage");
            var ulGradle = GetUnityLibraryBuildGradle(location);
            var sGradle = GetSettingsGradle(location);

            StringAssert.Contains(DummyDependencyInGradle, ulGradle);
            StringAssert.Contains(DummyRepositoryInGradle, sGradle);

            DeleteAsset(kPackageTempPath);

            location = BuildProject("DepsInPackage");
            ulGradle = GetUnityLibraryBuildGradle(location);
            sGradle = GetSettingsGradle(location);

            StringAssert.DoesNotContain(DummyDependencyInGradle, ulGradle);
            StringAssert.DoesNotContain(DummyRepositoryInGradle, sGradle);
        }
        private string CreateLocalRepository(string name)
        {
            var root = Path.Combine(kAssetsPath, name);
            var repoRoot = Path.Combine(root, "m2repository/com/unity/test/test-unity");
            var mavenMetaData = Path.Combine(repoRoot, "maven-metadata.xml");

            CreateAssetWithTextContent(mavenMetaData,
                @"<metadata>
  <groupId>com.unity.test</groupId>
  <artifactId>test-unity</artifactId>
  <versioning>
    <release>11.6.0</release>
    <versions><version>11.6.0</version></versions>
    <lastUpdated/>
  </versioning>
</metadata>
");
            return root;
        }

        [Test]
        public void CanInjectDependenciesWithLocalRepoInAssets()
        {
            CreateAssetWithTextContent(kAssetsTempPath, "dependencies.xml", CreateDummyXmlContents(DummyDependency, DummyRepository));
        }
    }
}
