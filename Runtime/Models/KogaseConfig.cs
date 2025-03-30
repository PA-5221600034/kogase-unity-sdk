using System;

namespace Kogase.Models
{
    /// <summary>
    /// Configuration for the Kogase SDK
    /// </summary>
    [Serializable]
    public class KogaseConfig
    {
        /// <summary>
        /// The base URL of the Kogase API server
        /// </summary>
        public string BaseUrl = "http://localhost:8080";

        /// <summary>
        /// API key for the project
        /// </summary>
        public string ApiKey = "";

        /// <summary>
        /// API version path
        /// </summary>
        public string ApiVersion = "v1";

        /// <summary>
        /// Maximum number of cached events before auto-sending
        /// </summary>
        public int MaxCachedEvents = 50;

        /// <summary>
        /// Whether to enable logging for debug purposes
        /// </summary>
        public bool EnableDebugLogging = false;

        /// <summary>
        /// Whether to automatically track sessions
        /// </summary>
        public bool AutoTrackSessions = true;

        /// <summary>
        /// Whether to automatically cache events when offline and send when back online
        /// </summary>
        public bool EnableOfflineCache = true;

        public string GetBackendUrl()
        {
            return $"{BaseUrl}/api/{ApiVersion}";
        }

        /// <summary>
        /// Clone the configuration
        /// </summary>
        /// <returns>A new instance with the same values</returns>
        public KogaseConfig Clone()
        {
            return new KogaseConfig
            {
                BaseUrl = BaseUrl,
                ApiKey = ApiKey,
                ApiVersion = ApiVersion,
                MaxCachedEvents = MaxCachedEvents,
                EnableDebugLogging = EnableDebugLogging,
                AutoTrackSessions = AutoTrackSessions,
                EnableOfflineCache = EnableOfflineCache
            };
        }
    }
}