using Kogase.Models;
using System;
using System.IO;
using Kogase.Dtos;
using UnityEditor;
using UnityEngine;
namespace Kogase.Editor
{
    /// <summary>
    /// Window for editing Kogase SDK settings
    /// </summary>
    public class KogaseSettingsEditor : EditorWindow
    {
        static KogaseSettingsEditor _instance;
        public static KogaseSettingsEditor Instance => _instance;

        const string KWindowTitle = "Kogase SDK Settings";

        string configFilePath;
        KogaseConfig originalConfig;
        KogaseConfig editedConfig;
        CreateProjectRequest createProjectRequest;
        Vector2 scrollPosition;
        bool isDirty;
        bool isInitialized;
        bool isDoingOperation;
        
        [MenuItem("Kogase/Edit Settings")]
        public static void OpenWindow()
        {
            if (_instance != null)
            {
                _instance.CloseFinal();
            }
            
            _instance = GetWindow<KogaseSettingsEditor>(KWindowTitle, true, Type.GetType("UnityEditor.InspectorWindow, UnityEditor.dll"));
            _instance.Show();
        }
        
        void Initialize()
        {
            if (isInitialized)
                return;
            
            isInitialized = true;
                
            configFilePath = Path.Combine("Assets/Resources/Kogase", "KogaseSDKConfig.json");

            if (originalConfig == null)
            {
                originalConfig = KogaseSettings.SDKConfig;
                
                if (originalConfig == null)
                {
                    originalConfig = new KogaseConfig();
                    string json = JsonUtility.ToJson(originalConfig, true);
                    File.WriteAllText(configFilePath, json);
                    AssetDatabase.Refresh();
                }
            }
            
            editedConfig ??= originalConfig.Clone();
        }
        
        void CloseFinal()
        {
            Close();
            _instance = null;
        }

        void OnGUI()
        {
            Initialize();
            
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(
                "Kogase SDK Settings", 
                new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 16,
                    alignment = TextAnchor.MiddleCenter
                }
            );
            EditorGUILayout.Space(10);
            
            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Editor Deactivated On Runtime", MessageType.Info, wide: true);
                EditorGUILayout.EndVertical();
                return;
            }
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, alwaysShowHorizontal:false, alwaysShowVertical:false);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            
            EditorGUILayout.LabelField(
                $"SDK Version {KogaseSettings.SDKVersion}", 
                new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter }
            );
            
            EditorGUI.indentLevel++;

            string baseUrl = EditorGUILayout.TextField("Base URL", editedConfig.BaseUrl);
            if (baseUrl != editedConfig.BaseUrl)
            {
                editedConfig.BaseUrl = baseUrl;
                isDirty = true;
            }
            
            string apiKey = EditorGUILayout.TextField("API Key", editedConfig.ApiKey);
            if (apiKey != editedConfig.ApiKey)
            {
                editedConfig.ApiKey = apiKey;
                isDirty = true;
            }
            
            string apiVersion = EditorGUILayout.TextField("API Version", editedConfig.ApiVersion);
            if (apiVersion != editedConfig.ApiVersion)
            {
                editedConfig.ApiVersion = apiVersion;
                isDirty = true;
            }
            
            int maxCachedEvents = EditorGUILayout.IntField("Max Cached Events", editedConfig.MaxCachedEvents);
            if (maxCachedEvents != editedConfig.MaxCachedEvents)
            {
                editedConfig.MaxCachedEvents = maxCachedEvents;
                isDirty = true;
            }
            
            bool enableDebugLogging = EditorGUILayout.Toggle("Enable Debug Log", editedConfig.EnableDebugLogging);
            if (enableDebugLogging != editedConfig.EnableDebugLogging)
            {
                editedConfig.EnableDebugLogging = enableDebugLogging;
                isDirty = true;
            }
            
            bool autoTrackSessions = EditorGUILayout.Toggle("Auto-Track Sessions", editedConfig.AutoTrackSessions);
            if (autoTrackSessions != editedConfig.AutoTrackSessions)
            {
                editedConfig.AutoTrackSessions = autoTrackSessions;
                isDirty = true;
            }
            
            bool enableOfflineCache = EditorGUILayout.Toggle("Enable Offline Cache", editedConfig.EnableOfflineCache);
            if (enableOfflineCache != editedConfig.EnableOfflineCache)
            {
                editedConfig.EnableOfflineCache = enableOfflineCache;
                isDirty = true;
            }

            EditorGUI.indentLevel--;

            using (new EditorGUI.DisabledGroupScope(isDoingOperation))
            {
                if (GUILayout.Button("Test Connection"))
                {
                    isDoingOperation = true;
                    Repaint();

                    KogaseSDK.TestConnection(
                        ok =>
                        {
                            isDoingOperation = false;
                            EditorUtility.DisplayDialog(
                                "Test Connection", 
                                "Connection successful!", 
                                "OK"
                            );
                            Repaint();
                        },
                        error =>
                        {
                            isDoingOperation = false;
                            EditorUtility.DisplayDialog(
                                "Test Connection", 
                                $"Connection failed: {error.Message}",
                                "OK"
                            );
                            Repaint();
                        }
                    );
                }
            }

            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField(
                "Create Project Here (optional)", 
                new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter }
            );
            
            EditorGUI.indentLevel++;
            
            string projectName = EditorGUILayout.TextField("Project Name", createProjectRequest.Name);
            if (projectName != createProjectRequest.Name)
            {
                createProjectRequest.Name = projectName;
                Repaint();
            }
            
            EditorGUI.indentLevel--;
            
            using (new EditorGUI.DisabledGroupScope(isDoingOperation || string.IsNullOrEmpty(projectName)))
            {
                if (GUILayout.Button("Create Project"))
                {
                    isDoingOperation = true;
                    Repaint();

                    KogaseSDK.CreateProject(
                        createProjectRequest,
                        ok =>
                        {
                            isDoingOperation = false;
                            EditorUtility.DisplayDialog(
                                "Create Project", 
                                "Project created successfully!", 
                                "OK"
                            );
                            Debug.Log(ok.ApiKey);
                            editedConfig.ApiKey = ok.ApiKey;
                            Repaint();
                        },
                        error =>
                        {
                            isDoingOperation = false;
                            EditorUtility.DisplayDialog(
                                "Create Project", 
                                $"Failed to create project: {error.Message}",
                                "OK"
                            );
                            Repaint();
                        }
                    );
                }
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.EndVertical();
            
            if (isDoingOperation)
            {
                EditorGUILayout.HelpBox("Wait for a minute...", MessageType.Info);
            }
            
            if (isDirty)
            {
                EditorGUILayout.HelpBox("You have unsaved changes", MessageType.Warning);
            }
            
            if (GUILayout.Button("Reset"))
            {
                ResetConfig();
            }
            
            GUI.enabled = isDirty;
            if (GUILayout.Button("Save"))
            {
                SaveConfig();
            }
        }

        void ResetConfig()
        {
            editedConfig = originalConfig.Clone();
            
            isDirty = false;
            GUI.FocusControl(null);
            Repaint();
        }

        void SaveConfig()
        {
            try
            {
                originalConfig = editedConfig.Clone();
                KogaseSettings.SaveSDKConfigFile(originalConfig);
                
                isDirty = false;
                GUI.FocusControl(null);
                Repaint();
                
                EditorUtility.DisplayDialog("Kogase SDK", "Config saved successfully", "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Kogase SDK", $"Failed to save config: {ex.Message}", "OK");
            }
        }
    }
} 