using Kogase.Core;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Kogase.Editor
{
    /// <summary>
    /// Window for editing Kogase SDK settings
    /// </summary>
    public class KogaseSettingsEditorWindow : EditorWindow
    {
        const string KWindowTitle = "Kogase SDK Settings";
        
        static KogaseSettingsEditorWindow _instance;

        string configFilePath;
        KogaseConfig originalConfig;
        KogaseConfig editedConfig;
        Vector2 scrollPosition;
        bool isDirty;
        
        GUIStyle headerStyle;
        GUIStyle requiredStyle;
        GUIStyle sectionHeaderStyle;
        
        [MenuItem("Kogase/Settings")]
        public static void OpenWindow()
        {
            if (_instance != null)
            {
                _instance.Close();
            }
            
            _instance = GetWindow<KogaseSettingsEditorWindow>(true, KWindowTitle);
            _instance.minSize = new Vector2(450, 500);
            _instance.Show();
        }

        void OnEnable()
        {
            configFilePath = Path.Combine("Assets/Resources/Kogase", "kogaseconfig.json");
            
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.boldLabel);
                headerStyle.fontSize = 14;
                headerStyle.alignment = TextAnchor.MiddleCenter;
                headerStyle.margin = new RectOffset(0, 0, 10, 10);
            }
            
            if (requiredStyle == null)
            {
                requiredStyle = new GUIStyle(EditorStyles.label);
                requiredStyle.normal.textColor = new Color(1.0f, 0.5f, 0.5f);
            }
            
            if (sectionHeaderStyle == null)
            {
                sectionHeaderStyle = new GUIStyle(EditorStyles.boldLabel);
                sectionHeaderStyle.margin = new RectOffset(0, 0, 10, 5);
            }
            
            LoadConfig();
        }

        void LoadConfig()
        {
            originalConfig = null;
            
            // Try to load from Resources
            TextAsset configAsset = Resources.Load<TextAsset>("Kogase/kogaseconfig");
            if (configAsset != null)
            {
                try
                {
                    originalConfig = JsonUtility.FromJson<KogaseConfig>(configAsset.text);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to parse Kogase config: {ex.Message}");
                }
            }
            
            // If not in resources, try loading from the file
            if (originalConfig == null && File.Exists(configFilePath))
            {
                try
                {
                    string json = File.ReadAllText(configFilePath);
                    originalConfig = JsonUtility.FromJson<KogaseConfig>(json);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to load Kogase config from file: {ex.Message}");
                }
            }
            
            originalConfig ??= new KogaseConfig();
            
            // Create a copy for editing
            editedConfig = originalConfig.Clone();
            isDirty = false;
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Kogase SDK Configuration", headerStyle);
            GUILayout.Space(20);
            
            // Show save status
            if (isDirty)
            {
                EditorGUILayout.HelpBox("You have unsaved changes", MessageType.Warning);
            }
            
            // Start scrollable area
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // Backend Configuration Section
            EditorGUILayout.LabelField("API Configuration", sectionHeaderStyle);
            EditorGUILayout.Space(5);
            
            string newBackendUrl = EditorGUILayout.TextField(new GUIContent("Backend URL", "Base URL for the Kogase API"), editedConfig.BackendUrl);
            if (newBackendUrl != editedConfig.BackendUrl)
            {
                editedConfig.BackendUrl = newBackendUrl;
                isDirty = true;
            }
            
            // API key with required field marking
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("API Key", "Your project's API key for authentication"), GUILayout.Width(150));
            string newApiKey = EditorGUILayout.TextField(editedConfig.ApiKey);
            if (newApiKey != editedConfig.ApiKey)
            {
                editedConfig.ApiKey = newApiKey;
                isDirty = true;
            }
            if (string.IsNullOrEmpty(editedConfig.ApiKey))
            {
                EditorGUILayout.LabelField("Required", requiredStyle, GUILayout.Width(70));
            }
            else
            {
                EditorGUILayout.LabelField("", GUILayout.Width(70));
            }
            EditorGUILayout.EndHorizontal();
            
            string newApiVersion = EditorGUILayout.TextField(new GUIContent("API Version", "Version of the API to use"), editedConfig.ApiVersion);
            if (newApiVersion != editedConfig.ApiVersion)
            {
                editedConfig.ApiVersion = newApiVersion;
                isDirty = true;
            }
            
            EditorGUILayout.Space(15);
            
            // Settings Section
            EditorGUILayout.LabelField("SDK Settings", sectionHeaderStyle);
            EditorGUILayout.Space(5);
            
            bool newEnableDebugLogging = EditorGUILayout.Toggle(new GUIContent("Enable Debug Logging", "Enable detailed logging for debugging"), editedConfig.EnableDebugLogging);
            if (newEnableDebugLogging != editedConfig.EnableDebugLogging)
            {
                editedConfig.EnableDebugLogging = newEnableDebugLogging;
                isDirty = true;
            }
            
            bool newAutoTrackSessions = EditorGUILayout.Toggle(new GUIContent("Auto-Track Sessions", "Automatically track user sessions"), editedConfig.AutoTrackSessions);
            if (newAutoTrackSessions != editedConfig.AutoTrackSessions)
            {
                editedConfig.AutoTrackSessions = newAutoTrackSessions;
                isDirty = true;
            }
            
            bool newEnableOfflineCache = EditorGUILayout.Toggle(new GUIContent("Enable Offline Cache", "Cache events when offline and send them later"), editedConfig.EnableOfflineCache);
            if (newEnableOfflineCache != editedConfig.EnableOfflineCache)
            {
                editedConfig.EnableOfflineCache = newEnableOfflineCache;
                isDirty = true;
            }
            
            int newMaxCachedEvents = EditorGUILayout.IntField(new GUIContent("Max Cached Events", "Maximum number of cached events before auto-sending"), editedConfig.MaxCachedEvents);
            if (newMaxCachedEvents != editedConfig.MaxCachedEvents)
            {
                editedConfig.MaxCachedEvents = newMaxCachedEvents;
                isDirty = true;
            }
            
            // End scrollable area
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space(10);
            
            // Save and Cancel buttons
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Reset", GUILayout.Width(100)))
            {
                LoadConfig();
            }
            
            GUI.enabled = isDirty;
            if (GUILayout.Button("Save", GUILayout.Width(100)))
            {
                SaveConfig();
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
        }

        void SaveConfig()
        {
            try
            {
                // Convert to JSON
                string json = JsonUtility.ToJson(editedConfig, true); // Pretty print
                
                // Create directory if it doesn't exist
                string directory = Path.GetDirectoryName(configFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // Write to file
                File.WriteAllText(configFilePath, json);
                
                // Refresh the Asset Database to show the file in the Project window
                AssetDatabase.Refresh();
                
                // Update original config (no need to reload)
                originalConfig = editedConfig.Clone();
                isDirty = false;
                
                Debug.Log($"Kogase config saved to {configFilePath}");
                
                if (KogaseSDK.Instance != null)
                {
                    KogaseSDK.Instance.UpdateConfig(editedConfig);
                    Debug.Log("Updated Kogase config");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save Kogase config: {ex.Message}");
                EditorUtility.DisplayDialog("Save Error", $"Failed to save config: {ex.Message}", "OK");
            }
        }
    }
} 