using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Kogase.Dtos;
using Kogase.Models;
using Kogase.Utils;
using UnityEngine;
using UnityEngine.Scripting;

namespace Kogase.Core
{
    /// <summary>
    /// Manages the caching of events when offline
    /// </summary>
    public class EventCache
    {
        const string CacheFileName = "EventCache.json";
        const string CacheDirectory = "Kogase/Cache";
        readonly string _fullCachePath;

        readonly KogaseConfig _config;
        readonly List<RecordEventRequest> _pendingEvents = new();
        readonly IFileStream _fileStream;

        public EventCache(KogaseConfig config, IFileStream fileStream)
        {
            _config = config;
            _fileStream = fileStream;

            _fullCachePath = Path.Combine(CommonInfo.PersistentPath, CacheDirectory, CacheFileName);

            // Load any cached events
            LoadEventsFromCache();
        }

        /// <summary>
        /// Add an event to the cache
        /// </summary>
        public void CacheEvent(RecordEventRequest eventRequest)
        {
            if (!_config.EnableOfflineCache) return;

            lock (_pendingEvents)
            {
                _pendingEvents.Add(eventRequest);

                // If we've hit the max cache size, save to disk
                if (_pendingEvents.Count >= _config.MaxCachedEvents) SaveEventsToCache();
            }
        }

        /// <summary>
        /// Get all pending events and clear the cache
        /// </summary>
        public List<RecordEventRequest> GetAndClearPendingEvents()
        {
            List<RecordEventRequest> events;

            lock (_pendingEvents)
            {
                events = new List<RecordEventRequest>(_pendingEvents);
                _pendingEvents.Clear();

                // Clear the cache file too
                ClearCache();
            }

            return events;
        }

        /// <summary>
        /// Save pending events to the cache file
        /// </summary>
        public void SaveEventsToCache()
        {
            if (!_config.EnableOfflineCache || _pendingEvents.Count == 0) return;

            try
            {
                // Convert events to JSON
                var json = JsonUtility.ToJson(new EventCacheData { Events = _pendingEvents });

                // Ensure directory exists
                var directory = Path.GetDirectoryName(_fullCachePath);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                // Write to cache file
                _fileStream.WriteFile(null, json, _fullCachePath, null, true);

                Debug.Log($"Saved {_pendingEvents.Count} events to cache");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to save events to cache: {e.Message}");
            }
        }

        /// <summary>
        /// Load events from the cache file
        /// </summary>
        void LoadEventsFromCache()
        {
            if (!_config.EnableOfflineCache) return;

            try
            {
                if (_fileStream.IsFileExist(_fullCachePath))
                    _fileStream.ReadFile(null, _fullCachePath, (success, content) =>
                    {
                        if (success && !string.IsNullOrEmpty(content))
                            try
                            {
                                var cacheData = JsonUtility.FromJson<EventCacheData>(content);
                                if (cacheData != null && cacheData.Events != null)
                                {
                                    lock (_pendingEvents)
                                    {
                                        _pendingEvents.AddRange(cacheData.Events);
                                    }

                                    Debug.Log($"Loaded {cacheData.Events.Count} cached events");
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning($"Failed to parse cached events: {e.Message}");
                            }
                    }, true);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load cached events: {e.Message}");
            }
        }

        /// <summary>
        /// Clear the cache file
        /// </summary>
        void ClearCache()
        {
            if (!_config.EnableOfflineCache) return;

            try
            {
                if (_fileStream.IsFileExist(_fullCachePath)) _fileStream.DeleteFile(_fullCachePath, null, true);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to clear event cache: {e.Message}");
            }
        }
    }

    [DataContract]
    [Preserve]
    public class EventCacheData
    {
        [DataMember(Name = "events")] public List<RecordEventRequest> Events;
    }
}