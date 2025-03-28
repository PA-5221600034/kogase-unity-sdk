using Kogase.Core;
using Kogase.Models;
using System.Collections;

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

        #region Project Management

        /// <summary>
        /// Gets the current project information using API key
        /// </summary>
        /// <returns>Project information</returns>
        public IEnumerator GetProjectAsync()
        {
            var request = HttpRequestBuilder.CreateGet(BaseUrl + "/projects/apikey")
                .WithApiKeyAuth()
                .Accepts(MediaType.ApplicationJson)
                .GetResult();

            IHttpResponse response = null;

            yield return HttpClient.SendRequest(request,
                rsp => response = rsp);

            // var result = response.TryParseJson<>();
            // callback?.Try(result);
        }

        #endregion Project Management
    }
}