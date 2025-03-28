using System;
using Kogase.Api;
using Kogase.Core;
using Kogase.Models;
using Kogase.Utils;

namespace Kogase
{
    internal class KogaseSDKImpl
    {
        CommonApi api;

        HeartBeat heartBeat;
        HeartBeat HeartBeat => heartBeat ??= new HeartBeat();

        public string Version => KogaseSettings.SDKVersion;

        readonly KogaseServiceTracker serviceTracker;

        // TODO: maybe this not needed
        //
        internal Action<IHttpClient> OnHttpClientCreated;
        //

        public KogaseSDKImpl()
        {
            serviceTracker = new KogaseServiceTracker();
            var serviceLogger = new KogaseServiceLogger();

            serviceTracker.OnNewRequestSentEvent += serviceLogger.LogServiceActivity;
            serviceTracker.OnNewResponseReceivedEvent += serviceLogger.LogServiceActivity;
            serviceTracker.OnSendingWebsocketRequestEvent += serviceLogger.LogServiceActivity;
            serviceTracker.OnReceivingWebsocketNotificationEvent += serviceLogger.LogServiceActivity;
        }

        public KogaseConfig GetConfig()
        {
            var retval = KogaseSettings.SDKConfig;
            retval = retval.Clone();
            return retval;
        }

        internal void Reset()
        {
            // TODO: Implement reset logic

            if (heartBeat != null)
            {
                heartBeat.Reset();
                heartBeat = null;
            }
        }

        #region File Stream

        IFileStream fileStream;

        internal IFileStream FileStream
        {
            get
            {
                if (fileStream == null) fileStream = CreateFileStream();
                return fileStream;
            }
        }

        public IFileStreamFactory FileStreamFactory;

        IFileStream CreateFileStream()
        {
            var fileStreamFactory = FileStreamFactory ?? new FileStreamFactory();

            var createdFileStream = fileStreamFactory.CreateFileStream();

            KogaseSDKMain.OnUpdate += dt => { createdFileStream.Pop(); };

            return createdFileStream;
        }

        internal void DisposeFileStream()
        {
            if (fileStream != null) fileStream.Dispose();
        }

        #endregion File Stream

        #region HTTP

        IHttpRequestSenderFactory sdkHttpSenderFactory;

        internal IHttpRequestSenderFactory SdkHttpSenderFactory
        {
            get
            {
                if (sdkHttpSenderFactory == null)
                {
                    var defaultHttpSender = new HttpRequestFactory(HeartBeat);
                    defaultHttpSender.OnWebRequestSchedulerCreated = serviceTracker.OnNewWebRequestSchedulerCreated;
                    sdkHttpSenderFactory = defaultHttpSender;
                }

                return sdkHttpSenderFactory;
            }
            set => sdkHttpSenderFactory = value;
        }

        #endregion HTTP

        // /// <summary>
        // /// Queue for storing events when offline
        // /// </summary>
        // Queue<Dictionary<string, object>> eventCache;
        //
        // /// <summary>
        // /// Coroutine runner for asynchronous operations
        // /// </summary>
        // CoroutineRunner coroutineRunner;
        //
        //
        // /// <summary>
        // /// Private constructor for singleton pattern
        // /// </summary>
        // public KogaseSDKImpl()
        // {
        //     eventCache = new Queue<Dictionary<string, object>>();
        //     coroutineRunner = new CoroutineRunner();
        // }
        //
        // /// <summary>
        // /// Initializes the SDK with default configuration
        // /// </summary>
        // public void Initialize()
        // {
        //     if (IsInitialized)
        //     {
        //         Debug.LogWarning("Kogase SDK is already initialized");
        //         return;
        //     }
        //     
        //     api = new CommonApi(KogaseSettings.SDKConfig);
        //     
        //     IsInitialized = true;
        //     
        //     if (KogaseSettings.SDKConfig.AutoTrackSessions)
        //     {
        //         StartSession();
        //     }
        // }
        //
        // /// <summary>
        // /// Starts a new session
        // /// </summary>
        // public void StartSession()
        // {
        //     EnsureInitialized();
        //     
        //     // Track session start event
        //     Dictionary<string, object> properties = new Dictionary<string, object>
        //     {
        //         { "type", "session_start" },
        //         { "platform", Application.platform.ToString() },
        //         { "device_model", SystemInfo.deviceModel },
        //         { "os_version", SystemInfo.operatingSystem },
        //         { "app_version", Application.version }
        //     };
        //     
        //     api.RecordEvent("session_start", properties, null);
        // }
        //
        // /// <summary>
        // /// Ends the current session
        // /// </summary>
        // public void EndSession()
        // {
        //     EnsureInitialized();
        //     
        //     // Track session end event
        //     Dictionary<string, object> properties = new Dictionary<string, object>
        //     {
        //         { "type", "session_end" },
        //         { "duration", Time.time }
        //     };
        //     
        //     api.RecordEvent("session_end", properties, null);
        // }
        //
        // /// <summary>
        // /// Tracks a custom event
        // /// </summary>
        // /// <param name="eventName">Name of the event</param>
        // /// <param name="properties">Event properties</param>
        // public void TrackEvent(string eventName, Dictionary<string, object> properties = null)
        // {
        //     EnsureInitialized();
        //     
        //     if (string.IsNullOrEmpty(eventName))
        //     {
        //         Debug.LogError("Event name cannot be null or empty");
        //         return;
        //     }
        //     
        //     api.RecordEvent(eventName, properties, (success, error) =>
        //     {
        //         if (!success && KogaseSettings.SDKConfig.EnableOfflineCache)
        //         {
        //             // Cache event for later if offline
        //             Dictionary<string, object> eventData = new Dictionary<string, object>
        //             {
        //                 { "name", eventName },
        //                 { "properties", properties ?? new Dictionary<string, object>() },
        //                 { "timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }
        //             };
        //             
        //             eventCache.Enqueue(eventData);
        //             
        //             if (KogaseSettings.SDKConfig.EnableDebugLogging)
        //             {
        //                 Debug.Log($"Kogase: Event {eventName} cached for later sending");
        //             }
        //             
        //             // Auto-send if we have enough events
        //             if (eventCache.Count >= KogaseSettings.SDKConfig.MaxCachedEvents)
        //             {
        //                 FlushEvents();
        //             }
        //         }
        //         else if (KogaseSettings.SDKConfig.EnableDebugLogging)
        //         {
        //             if (success)
        //             {
        //                 Debug.Log($"Kogase: Event {eventName} tracked successfully");
        //             }
        //             else
        //             {
        //                 Debug.LogError($"Kogase: Failed to track event {eventName}: {error.Message}");
        //             }
        //         }
        //     });
        // }
        //
        // /// <summary>
        // /// Sends all cached events
        // /// </summary>
        // public void FlushEvents()
        // {
        //     EnsureInitialized();
        //     
        //     if (eventCache.Count == 0)
        //     {
        //         return;
        //     }
        //     
        //     List<EventData> events = new List<EventData>();
        //     
        //     while (eventCache.Count > 0)
        //     {
        //         var eventData = eventCache.Dequeue();
        //         
        //         events.Add(new EventData
        //         {
        //             Name = (string)eventData["name"],
        //             Properties = (Dictionary<string, object>)eventData["properties"],
        //             Timestamp = (long)eventData["timestamp"]
        //         });
        //     }
        //     
        //     api.TrackEventsAsync(events);
        // }
        //
        // /// <summary>
        // /// Ensures the SDK is initialized
        // /// </summary>
        // void EnsureInitialized()
        // {
        //     if (!IsInitialized)
        //     {
        //         throw new InvalidOperationException("Kogase SDK is not initialized. Call Initialize() first.");
        //     }
        // }
    }
}