using Kogase.Core;
using System;

namespace Kogase.Utils
{
    internal class KogaseServiceTracker
    {
        internal Action<ServiceLog, IDebugger> OnNewRequestSentEvent, OnNewResponseReceivedEvent, OnSendingWebsocketRequestEvent, OnReceivingWebsocketNotificationEvent;

        public void OnNewWebRequestSchedulerCreated(WebRequestScheduler newScheduler)
        {
            newScheduler.PreHttpRequest += (webRequest, headers, payload, logger) =>
            {
                bool enhancedLoggingEnabled = logger != null ? logger.IsEnhancedLoggingEnabled() : false;
                if (enhancedLoggingEnabled && !string.IsNullOrEmpty(webRequest.RequestId))
                {
                    var requestData = new ServiceRequestData()
                    {
                        Verb = webRequest.method.ToUpper(),
                        Url = webRequest.url,
                        Header = headers
                    };
                    if (payload != null)
                    {
                        requestData.Payload = System.Text.Encoding.UTF8.GetString(payload);
                    }
                    
                    var requestLog = new ServiceLog()
                    {
                        MessageId = $"{webRequest.RequestId}",
                        Timestamp = webRequest.RequestTimestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture),
                        Type = (int) DataType.REQUEST,
                        Direction = (int) DataDirection.SENDING,
                        Data = requestData
                    };
                    OnNewRequestSentEvent?.Invoke(requestLog, logger);
                }
            };
            
            newScheduler.PostHttpRequest += (webRequest, logger) =>
            {
                bool enhancedLoggingEnabled = logger != null ? logger.IsEnhancedLoggingEnabled() : false;
                if (enhancedLoggingEnabled && !string.IsNullOrEmpty(webRequest.RequestId))
                {
                    var requestData = new ServiceResponseData()
                    {
                        Verb = webRequest.method.ToUpper(),
                        Url = webRequest.url,
                        Header = webRequest.GetResponseHeaders(),
                        Status = webRequest.responseCode,
                    };
                    if (webRequest.downloadHandler.data != null)
                    {
                        requestData.Payload = System.Text.Encoding.UTF8.GetString(webRequest.downloadHandler.data);
                    }

                    string responseContentType = webRequest.GetResponseHeader(KogaseWebRequest.ResponseContentTypeHeader);
                    if (!string.IsNullOrEmpty(responseContentType))
                    {
                        requestData.ContentType = responseContentType;
                    }
                    
                    var requestLog = new ServiceLog()
                    {
                        MessageId = $"{webRequest.RequestId}",
                        Timestamp = webRequest.ResponseTimestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture),
                        Type = (int) DataType.REQUEST,
                        Direction = (int) DataDirection.RECEIVING,
                        Data = requestData
                    };
                    OnNewResponseReceivedEvent?.Invoke(requestLog, logger);
                }
            };
        }

        public void OnSendingWebsocketRequest(string message, IDebugger logger)
        {
            bool enhancedLoggingEnabled = logger != null ? logger.IsEnhancedLoggingEnabled() : false;
            if (enhancedLoggingEnabled)
            {
                var requestData = new WebsocketData()
                {
                    Payload = message
                };
                    
                var requestLog = new ServiceLog()
                {
                    MessageId = $"{Guid.NewGuid().ToString()}",
                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture),
                    Type = (int) DataType.WEBSOCKET_NOTIFICATION,
                    Direction = (int) DataDirection.SENDING,
                    Data = requestData
                };
                OnSendingWebsocketRequestEvent?.Invoke(requestLog, logger);
            }
        }
        
        public void OnReceivingWebsocketNotification(string message, IDebugger logger)
        {
            bool enhancedLoggingEnabled = logger != null ? logger.IsEnhancedLoggingEnabled() : false;
            if (enhancedLoggingEnabled)
            {
                var requestData = new WebsocketData()
                {
                    Payload = message
                };
                    
                var requestLog = new ServiceLog()
                {
                    MessageId = $"{Guid.NewGuid().ToString()}",
                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture),
                    Type = (int) DataType.WEBSOCKET_NOTIFICATION,
                    Direction = (int) DataDirection.RECEIVING,
                    Data = requestData
                };
                OnReceivingWebsocketNotificationEvent?.Invoke(requestLog, logger);
            }
        }
    }
}