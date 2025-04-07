using System.Collections.Generic;
using Kogase.Core;
using Kogase.Models;
using Kogase.Dtos;

namespace Kogase.Api
{
    public class CommonApi : ApiBase
    {
        public CommonApi(IHttpClient inHttpClient, KogaseConfig inConfig)
            : base(inHttpClient, inConfig)
        {
        }

        public void TestConnection(
            OkDelegate<HealthResponse> okCallback = null,
            ErrorDelegate<Error> errorCallback = null)
        {
            var request = HttpRequestBuilder.CreateGet(BaseUrl + "/health/apikey")
                .WithHeader("X-Kogase-API-Key", Config.ApiKey)
                .Accepts(HttpMediaType.ApplicationJson)
                .GetResult();

            HttpOperator.Send(request, okCallback, errorCallback);
        }

        public void CreateProject(
            CreateProjectRequest payload,
            OkDelegate<CreateProjectResponse> okCallback = null,
            ErrorDelegate<Error> errorCallback = null)
        {
            var request = HttpRequestBuilder.CreatePost(BaseUrl + "/projects")
                .WithJsonBody(payload)
                .Accepts(HttpMediaType.ApplicationJson)
                .GetResult();

            HttpOperator.Send(request, okCallback, errorCallback);
        }

        internal void CreateOrUpdateDevice(
            CreateOrUpdateDeviceRequest payload,
            OkDelegate<CreateOrUpdateDeviceResponse> okCallback = null,
            ErrorDelegate<Error> errorCallback = null)
        {
            var request = HttpRequestBuilder.CreatePost(BaseUrl + "/devices")
                .WithHeader("X-Kogase-API-Key", Config.ApiKey)
                .WithJsonBody(payload)
                .Accepts(HttpMediaType.ApplicationJson)
                .GetResult();

            HttpOperator.Send(request, okCallback, errorCallback);
        }

        #region Session

        internal void BeginSession(
            BeginSessionRequest request,
            OkDelegate<BeginSessionResponse> okCallback = null,
            ErrorDelegate<Error> errorCallback = null)
        {
            var httpRequest = HttpRequestBuilder.CreatePost(BaseUrl + "/sessions/begin")
                .WithHeader("X-Kogase-API-Key", Config.ApiKey)
                .WithJsonBody(request)
                .Accepts(HttpMediaType.ApplicationJson)
                .GetResult();

            HttpOperator.Send(httpRequest, okCallback, errorCallback);
        }

        internal void EndSession(FinishSessionRequest request,
            OkDelegate<FinishSessionResponse> okCallback = null,
            ErrorDelegate<Error> errorCallback = null)
        {
            var httpRequest = HttpRequestBuilder.CreatePost(BaseUrl + "/sessions/finish")
                .WithHeader("X-Kogase-API-Key", Config.ApiKey)
                .WithJsonBody(request)
                .Accepts(HttpMediaType.ApplicationJson)
                .GetResult();

            HttpOperator.Send(httpRequest, okCallback, errorCallback);
        }

        #endregion Session

        #region Event

        public void RecordEvent(
            RecordEventRequest payload,
            OkDelegate<RecordEventResponse> okCallback = null,
            ErrorDelegate<Error> errorCallback = null)
        {
            payload.Identifier ??= KogaseSDK.Implementation.GetOrCreateDeviceIdentifier();

            var request = HttpRequestBuilder.CreatePost(BaseUrl + "/events")
                .WithHeader("X-Kogase-API-Key", Config.ApiKey)
                .WithJsonBody(payload)
                .Accepts(HttpMediaType.ApplicationJson)
                .GetResult();

            HttpOperator.Send(
                request,
                okCallback,
                error =>
                {
                    KogaseSDK.Implementation.EventManager.CacheEvent(payload);
                    errorCallback?.Invoke(error);
                });
        }

        public void RecordEvents(
            List<RecordEventRequest> payloads,
            OkDelegate<RecordEventsResponse> okCallback = null,
            ErrorDelegate<Error> errorCallback = null)
        {
            if (payloads.Count == 0)
            {
                errorCallback?.Invoke(new Error(Code.BAD_REQUEST));
                return;
            }

            var httpRequest = HttpRequestBuilder.CreatePost(BaseUrl + "/events/batch")
                .WithHeader("X-Kogase-API-Key", Config.ApiKey)
                .WithJsonBody(payloads)
                .Accepts(HttpMediaType.ApplicationJson)
                .GetResult();

            HttpOperator.Send(httpRequest, okCallback, errorCallback);
        }

        #endregion Event
    }
}