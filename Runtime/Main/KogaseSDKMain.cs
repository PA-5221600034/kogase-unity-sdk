using UnityEngine;

namespace Kogase
{
    public static class KogaseSDKMain
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void StartSDK()
        {

#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += EditorApplicationPlayModeStateChanged;
#endif
        }
        
        static void StopSDK()
        {
        }
        
#if UNITY_EDITOR
        static void EditorApplicationPlayModeStateChanged(UnityEditor.PlayModeStateChange newState)
        {
            if (newState != UnityEditor.PlayModeStateChange.ExitingPlayMode) return;
            
            UnityEditor.EditorApplication.playModeStateChanged -= EditorApplicationPlayModeStateChanged;
            StopSDK();
        }
#endif
    }
}