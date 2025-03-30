using Kogase.Core;
using Kogase.Models;
using Kogase.Dtos;

namespace Kogase.Api
{
    /// <summary>
    /// Main API client for communicating with the Kogase backend
    /// </summary>
    public class CommonApi : ApiBase
    {
        public CommonApi(IHttpClient inHttpClient, KogaseConfig inConfig)
            : base(inHttpClient, inConfig)
        {
        }

        public void TestConnection(OkDelegate<HealthResponse> okCallback = null,
            ErrorDelegate<Error> errorCallback = null)
        {
            var request = HttpRequestBuilder.CreateGet(BaseUrl + "/health/apikey")
                .WithHeader("X-Kogase-API-Key", Config.ApiKey)
                .Accepts(HttpMediaType.ApplicationJson)
                .GetResult();

            HttpOperator.Send(request, okCallback, errorCallback);
        }

        public void CreateProject(CreateProjectRequest payload, OkDelegate<CreateProjectResponse> okCallback = null,
            ErrorDelegate<Error> errorCallback = null)
        {
            var request = HttpRequestBuilder.CreatePost(BaseUrl + "/projects")
                .WithJsonBody(payload)
                .Accepts(HttpMediaType.ApplicationJson)
                .GetResult();

            HttpOperator.Send(request, okCallback, errorCallback);
        }
    }
}