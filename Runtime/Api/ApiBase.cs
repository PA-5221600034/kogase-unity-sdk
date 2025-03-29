using Kogase.Core;
using Kogase.Models;
using UnityEngine.Assertions;

namespace Kogase.Api
{
    public abstract class ApiBase
    {
        readonly IHttpClient httpClient;

        protected readonly string BaseUrl;

        internal readonly KogaseConfig Config;

        internal readonly HttpOperator HttpOperator;

        protected ApiBase(IHttpClient inHttpClient, KogaseConfig inConfig, HttpOperator inHttpOperator = null)
        {
            Assert.IsNotNull(inHttpClient, $"Creating {GetType().Name} failed. Parameter `inHttpClient` is null");
            Assert.IsNotNull(inConfig, $"Creating {GetType().Name} failed. Parameter `inConfig` is null");

            httpClient = inHttpClient;
            Config = inConfig;
            HttpOperator = inHttpOperator ?? HttpOperator.CreateDefault(inHttpClient);

            BaseUrl = inConfig.GetBackendUrl();
        }
    }
}