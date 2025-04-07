using System;
using Kogase.Dtos;
using UnityEngine;

namespace Kogase.Core
{
    public class EventManager
    {
        readonly EventCache _eventCache;
        bool _isProcessingBatch;

        public EventManager(EventCache eventCache)
        {
            _eventCache = eventCache;
            TrySendCachedEvents();
        }

        public void CacheEvent(RecordEventRequest eventRequest)
        {
            if (KogaseSettings.SDKConfig.EnableOfflineCache) _eventCache.CacheEvent(eventRequest);
        }

        public void TrySendCachedEvents()
        {
            if (_isProcessingBatch) return;

            try
            {
                _isProcessingBatch = true;

                var pendingEvents = _eventCache.GetAndClearPendingEvents();

                if (pendingEvents.Count > 0)
                {
                    Debug.Log($"Attempting to send {pendingEvents.Count} cached events");

                    KogaseSDK.Api.RecordEvents(pendingEvents,
                        _ =>
                        {
                            Debug.Log("Successfully sent cached events");
                            _isProcessingBatch = false;
                        },
                        error =>
                        {
                            Debug.LogWarning($"Failed to send cached events: {error.Message}");
                            foreach (var eventRequest in pendingEvents)
                                CacheEvent(eventRequest);
                            _isProcessingBatch = false;
                        });
                }
                else
                {
                    _isProcessingBatch = false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error processing cached events: {ex.Message}");
                _isProcessingBatch = false;
            }
        }

        public void SavePendingEvents()
        {
            _eventCache.SaveEventsToCache();
        }
    }
}