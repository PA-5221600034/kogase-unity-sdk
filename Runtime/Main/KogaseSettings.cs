using System.IO;
using Kogase.Core;
using Kogase.Utils;
using UnityEditor;
using UnityEngine;

namespace Kogase
{
    public abstract class KogaseSettings
    {
        const string KDefaultGeneratedConfigsResourceDirectory = "";
        public static string GeneratedConfigsResourceDirectory => KDefaultGeneratedConfigsResourceDirectory;
        
        static string _sdkVersion;
        public static string SDKVersion => _sdkVersion ??= GetKogaseSDKPackageVersion();
        
        KogaseConfig sdkConfig;
        public KogaseConfig SDKConfig => sdkConfig ??= LoadSDKConfigFile() ?? new KogaseConfig();
        
#if UNITY_EDITOR
        public static string GeneratedConfigsDirectoryFullPath()
        {
            string retval = System.IO.Path.Combine(Application.dataPath, "Resources", GeneratedConfigsResourceDirectory);
            return retval;
        }
        
        public static string SDKConfigFullPath()
        {
            return System.IO.Path.Combine(GeneratedConfigsDirectoryFullPath(), "KogaseSDKConfig.json");
        }
#endif
        
        public static string SDKConfigResourcePath()
        {
            return Path.Combine(GeneratedConfigsResourceDirectory, "KogaseSDKConfig");
        }

        public static KogaseConfig LoadSDKConfigFile()
        {
            KogaseConfig retval = null;
            Object configFile = Resources.Load(SDKConfigResourcePath());

            if (configFile != null)
            {
                string configFileJsonText = ((TextAsset)configFile).text;
                retval = configFileJsonText.ToObject<KogaseConfig>();
            }
            
            return retval;
        }
        
        public static void SaveSDKConfigFile(KogaseConfig config)
        {
#if UNITY_EDITOR
            string json = JsonUtility.ToJson(config, true);
            string directory = GeneratedConfigsDirectoryFullPath();
            string filePath = SDKConfigFullPath();
            
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            File.WriteAllText(filePath, json);
            AssetDatabase.Refresh();
#endif
        }

        static string GetKogaseSDKPackageVersion()
        {
#if UNITY_EDITOR
            string[] x = AssetDatabase.FindAssets ( $"t:Script {nameof(KogaseSDKRuntimeFolderPathNavigator)}" );
            string unityAssetPath = AssetDatabase.GUIDToAssetPath(x[0]);
            string relativePath = unityAssetPath.StartsWith("Assets/") 
                ? unityAssetPath.Substring("Assets/".Length) 
                : unityAssetPath;
            string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, relativePath));
            string sdkPath = Path.GetDirectoryName(Path.GetDirectoryName(fullPath));
            
            if (sdkPath != null)
            {
                string packageFilePath = Path.Combine(sdkPath, "package.json");
                string packageFileJsonText = File.ReadAllText(packageFilePath);

                return packageFileJsonText.GetValueFromJsonString<string>("version");
            }
            return "0.0.0";
#endif
        }
    }
}