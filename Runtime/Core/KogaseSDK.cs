using System;
using System.Collections.Generic;
using System.IO;
using Kogase.Core;
using UnityEngine;

namespace Kogase
{
    public class KogaseSDK
    {
        static KogaseSDK _instance;
        public static KogaseSDK Instance
        {
            get { return _instance ??= new KogaseSDK(); }
        }

        KogaseConfig config;
        Queue<Dictionary<string, object>> eventQueue;

        KogaseSDK()
        {
            // Try to load from Resources
            TextAsset configAsset = Resources.Load<TextAsset>("Kogase/kogaseconfig");
            if (configAsset != null)
            {
                try
                {
                    config = JsonUtility.FromJson<KogaseConfig>(configAsset.text);
                    Debug.Log($"Kogase config loaded from {Path.Combine("Assets/Resources/Kogase", "kogaseconfig.json")}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to parse Kogase config: {ex.Message}");
                }
            }
            
            // If still null, create a new config
            if (config == null)
            {
                config = new KogaseConfig();
                string configFilePath = Path.Combine("Assets/Resources/Kogase", "kogaseconfig.json");
                string json = JsonUtility.ToJson(config, true);
                
                // Create directory if it doesn't exist
                string directory = Path.GetDirectoryName(configFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory!);
                }
                
                // Write to file
                File.WriteAllText(configFilePath, json);
                Debug.Log($"New Kogase config written to {configFilePath}");
            }
            
            eventQueue = new Queue<Dictionary<string, object>>();
        }

        public void UpdateConfig(KogaseConfig newConfig)
        {
            config = newConfig;
        }
    }
} 