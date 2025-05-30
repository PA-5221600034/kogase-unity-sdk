using System;
using Kogase.Bootstrap;
using Kogase.Core;
using UnityEngine;

namespace Kogase
{
    public static class KogaseSDKMain
    {
        static bool _isInitialized;

        static IMonoBehaviourSignaller _monoBehaviourSignaller;

        static Action<float> _onUpdate;

        internal static Action<float> OnUpdate
        {
            get
            {
                EnsureMonoBehaviourSignallerExist();
                return _onUpdate;
            }
            set
            {
                EnsureMonoBehaviourSignallerExist();
                _onUpdate = value;
            }
        }

        internal static Action OnSDKStopped;

        static void ExecuteBootstraps()
        {
            SdkInterfaceBootstrap.Execute();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void StartSDK()
        {
            _onUpdate = null;

            ExecuteBootstraps();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += EditorApplicationPlayModeStateChanged;
#endif
#if !UNITY_SWITCH
            Application.quitting += ApplicationQuitting;
#endif
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void LateStartSDK()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;
        }

        static void StopSDK()
        {
            OnSDKStopped?.Invoke();
            SdkInterfaceBootstrap.Stop();
            DetachMonoBehaviourSignaller();

            _isInitialized = false;

            OnSDKStopped = null;
        }

#if UNITY_EDITOR
        static void EditorApplicationPlayModeStateChanged(UnityEditor.PlayModeStateChange newState)
        {
            if (newState != UnityEditor.PlayModeStateChange.ExitingPlayMode) return;

            UnityEditor.EditorApplication.playModeStateChanged -= EditorApplicationPlayModeStateChanged;
            StopSDK();
        }
#endif

        static void AttachMonoBehaviourSignaller(IMonoBehaviourSignaller newSignaller)
        {
            if (_monoBehaviourSignaller != null) _monoBehaviourSignaller.OnUpdateSignal -= OnUpdateSignal;

            _monoBehaviourSignaller = newSignaller;

            if (_monoBehaviourSignaller != null)
            {
                _monoBehaviourSignaller.OnUpdateSignal -= OnUpdateSignal;
                _monoBehaviourSignaller.OnUpdateSignal += OnUpdateSignal;
            }
            else
            {
                Debug.LogWarning("MonoBehaviourSignaller set to null.");
            }
        }

        static void DetachMonoBehaviourSignaller()
        {
            if (_monoBehaviourSignaller != null) _monoBehaviourSignaller.OnUpdateSignal -= OnUpdateSignal;

            _monoBehaviourSignaller = null;
        }

        static void EnsureMonoBehaviourSignallerExist()
        {
            if (_monoBehaviourSignaller == null)
            {
                var signallerGameObject = Utils.KogaseGameObject.GetOrCreateGameObject();
                var signaller = signallerGameObject.GetComponent<MonoBehaviourSignaller>();
                if (signaller == null) signaller = signallerGameObject.AddComponent<MonoBehaviourSignaller>();
                AttachMonoBehaviourSignaller(signaller);
            }
        }

        internal static void AddOnUpdateListener(Action<float> listener,
            bool ensureMonoBehaviourSignallerExist = true)
        {
            if (ensureMonoBehaviourSignallerExist) EnsureMonoBehaviourSignallerExist();
            _onUpdate += listener;
        }

        internal static void RemoveOnUpdateListener(Action<float> listener)
        {
            _onUpdate -= listener;
        }

        static void OnUpdateSignal(float deltaTime)
        {
            _onUpdate?.Invoke(deltaTime);
        }

        static void ApplicationQuitting()
        {
            StopSDK();
        }
    }
}