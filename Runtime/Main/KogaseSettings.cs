using System.IO;
using Kogase.Models;
using Kogase.Utils;
using UnityEngine;

namespace Kogase
{
    public abstract class KogaseSettings
    {
        const string DefaultGeneratedDataResourceDir = "";

        static string _sdkVersion;
        public static string SDKVersion => _sdkVersion ??= GetKogaseSDKPackageVersion();

        static KogaseConfig _sdkConfig;
        public static KogaseConfig SDKConfig => _sdkConfig ??= LoadSDKConfigFile() ?? new KogaseConfig();

#if UNITY_EDITOR
        static string GeneratedDataFullPathDir()
        {
            var retval = Path.Combine(Application.dataPath, "Resources", DefaultGeneratedDataResourceDir);
            return retval;
        }

        static string SDKConfigFullPath()
        {
            return Path.Combine(GeneratedDataFullPathDir(), "KogaseSDKConfig.json");
        }
#endif

        static string SDKConfigResourcePath()
        {
            return Path.Combine(DefaultGeneratedDataResourceDir, "KogaseSDKConfig");
        }

        static KogaseConfig LoadSDKConfigFile()
        {
            KogaseConfig retval = null;
            var configFile = Resources.Load(SDKConfigResourcePath());

            if (configFile != null)
            {
                var configFileJsonText = ((TextAsset)configFile).text;
                retval = configFileJsonText.ToObject<KogaseConfig>();
            }

            return retval;
        }

        public static void SaveSDKConfigFile(KogaseConfig config)
        {
#if UNITY_EDITOR
            var json = JsonUtility.ToJson(config, true);
            var directory = GeneratedDataFullPathDir();
            var filePath = SDKConfigFullPath();

            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            File.WriteAllText(filePath, json);
            UnityEditor.AssetDatabase.Refresh();

            _sdkConfig = LoadSDKConfigFile();
#endif
        }

        static string GetKogaseSDKPackageVersion()
        {
#if UNITY_EDITOR
            var x = UnityEditor.AssetDatabase.FindAssets($"t:Script {nameof(KogaseSDKRuntimeFolderPathNavigator)}");
            var unityAssetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(x[0]);
            var relativePath = unityAssetPath.StartsWith("Assets/")
                ? unityAssetPath.Substring("Assets/".Length)
                : unityAssetPath.StartsWith("Packages/")
                    ? unityAssetPath.Substring("Packages/".Length)
                    : unityAssetPath;

            var fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, relativePath));
            var sdkPath = Path.GetDirectoryName(Path.GetDirectoryName(fullPath));

            if (sdkPath != null)
            {
                var packageFilePath = Path.Combine(sdkPath, "package.json");

                if (File.Exists(packageFilePath))
                {
                    var packageFileJsonText = File.ReadAllText(packageFilePath);
                    return packageFileJsonText.GetValueFromJsonString<string>("version");
                }
            }

            return "0.0.0";
#endif
            return "0.0.0";
        }
    }
}
