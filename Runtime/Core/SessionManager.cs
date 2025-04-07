using System;
using Kogase.Dtos;
using Kogase.Utils;
using UnityEngine;

namespace Kogase.Core
{
    public class SessionManager
    {
        string CurrentSessionId { get; set; }

        public bool IsSessionActive => !string.IsNullOrEmpty(CurrentSessionId);

        DateTime _sessionStartTime;

        public SessionManager()
        {
        }

        public void BeginSession(Action okCallback, Action errorCallback)
        {
            if (IsSessionActive)
            {
                Debug.LogWarning("Session already active, cannot begin a new one");
                return;
            }

            var deviceIdentifier = KogaseSDK.Implementation.GetOrCreateDeviceIdentifier();

            var createOrUpdateDeviceRequest = new CreateOrUpdateDeviceRequest
            {
                Identifier = deviceIdentifier,
                Platform = CommonInfo.PlatformName,
                PlatformVersion = CommonInfo.PlatformVersion,
                AppVersion = CommonInfo.AppVersion
            };

            KogaseSDK.Api.CreateOrUpdateDevice(createOrUpdateDeviceRequest,
                response =>
                {
                    Debug.Log($"Device created/updated with ID: {response.DeviceID}");

                    var beginSessionRequest = new BeginSessionRequest
                    {
                        Identifier = deviceIdentifier
                    };

                    KogaseSDK.Api.BeginSession(beginSessionRequest,
                        response =>
                        {
                            CurrentSessionId = response.SessionID;
                            _sessionStartTime = DateTime.UtcNow;

                            Debug.Log($"Session begun with ID: {CurrentSessionId}");
                            okCallback?.Invoke();
                        },
                        error =>
                        {
                            Debug.LogError($"Failed to begin session: {error.Message}");
                            errorCallback?.Invoke();
                        });
                },
                error =>
                {
                    Debug.LogError($"Failed to create/update device: {error.Message}");
                    errorCallback?.Invoke();
                });
        }

        public void EndSession()
        {
            if (!IsSessionActive)
            {
                Debug.LogWarning("No active session to end");
                return;
            }

            var duration = DateTime.UtcNow - _sessionStartTime;
            var durationSeconds = (long)duration.TotalSeconds;

            var request = new FinishSessionRequest
            {
                SessionID = CurrentSessionId
            };

            KogaseSDK.Api.EndSession(request,
                response =>
                {
                    Debug.Log($"Session {CurrentSessionId} ended after {durationSeconds} seconds");

                    var endedSessionId = CurrentSessionId;
                    CurrentSessionId = null;
                },
                error => { Debug.LogError($"Failed to end session: {error.Message}"); });
        }
    }
}