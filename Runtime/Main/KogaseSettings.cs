using System.IO;
using Kogase.Models;
using Kogase.Utils;
using UnityEngine;

namespace Kogase
{
    public abstract class KogaseSettings
    {
        const string KDefaultGeneratedDataResourceDirectory = "";
        
        static string _sdkVersion;
        public static string SDKVersion => _sdkVersion ??= GetKogaseSDKPackageVersion();
        
        KogaseConfig sdkConfig;
        public KogaseConfig SDKConfig => sdkConfig ??= LoadSDKConfigFile() ?? new KogaseConfig();
        
#if UNITY_EDITOR
        public static string GeneratedDataDirectoryFullPath()
        {
            string retval = Path.Combine(Application.dataPath, "Resources", KDefaultGeneratedDataResourceDirectory);
            return retval;
        }
        
        public static string SDKConfigFullPath()
        {
            return Path.Combine(GeneratedDataDirectoryFullPath(), "KogaseSDKConfig.json");
        }
#endif
        
        public static string SDKConfigResourcePath()
        {
            return Path.Combine(KDefaultGeneratedDataResourceDirectory, "KogaseSDKConfig");
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
            string directory = GeneratedDataDirectoryFullPath();
            string filePath = SDKConfigFullPath();
            
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            File.WriteAllText(filePath, json);
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        static string GetKogaseSDKPackageVersion()
        {
#if UNITY_EDITOR
            string[] x = UnityEditor.AssetDatabase.FindAssets ( $"t:Script {nameof(KogaseSDKRuntimeFolderPathNavigator)}" );
            string unityAssetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(x[0]);
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