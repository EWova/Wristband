using System.IO;

using UnityEditor;

using UnityEngine;

namespace EWova.Wristband.Editor
{
    [InitializeOnLoad]
    internal static class PkgVerGen
    {
        static PkgVerGen() { }

        internal const string PackageJsonGuid = "352491e575f34f44da9dc9aef9524291";
        internal const string PackageInfoGuid = "4d9612c24c9d9ed41b0c34901b23ac17";
        internal static void Generate()
        {
            var packagePath = AssetDatabase.GUIDToAssetPath(PackageJsonGuid);

            if (string.IsNullOrEmpty(packagePath))
            {
                Debug.LogError($"[PkgVerGen] Cannot resolve package.json GUID: {PackageJsonGuid}");
                return;
            }

            var infoPath = AssetDatabase.GUIDToAssetPath(PackageInfoGuid);

            if (string.IsNullOrEmpty(infoPath))
            {
                Debug.LogError($"[PkgVerGen] Cannot resolve PackageInfo.cs GUID: {PackageInfoGuid}");
                return;
            }

            if (!File.Exists(packagePath))
            {
                Debug.LogError($"[PkgVerGen] package.json not found: {packagePath}");
                return;
            }

            var json = File.ReadAllText(packagePath);
            var package = JsonUtility.FromJson<PackageJson>(json);

            var code = $@"// auto generated
namespace EWova.Wristband
{{
    internal static class PackageInfo
    {{
        public const string Name = ""{package.name}"";
        public const string Version = ""{package.version}"";
    }}
}}";

            if (!File.Exists(infoPath))
            {
                var dir = Path.GetDirectoryName(infoPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }

            var existing = File.Exists(infoPath)
                ? File.ReadAllText(infoPath)
                : null;

            if (existing == code)
                return;

            File.WriteAllText(infoPath, code);

            AssetDatabase.Refresh();
        }

        [System.Serializable]
        private class PackageJson
        {
            public string name;
            public string version;
        }
    }
}
