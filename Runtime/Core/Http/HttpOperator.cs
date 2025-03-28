using System;
using System.Text;
using UnityEngine;

namespace Kogase.Core
{
    public abstract class HttpOperator
    {
        public abstract IHttpClient HttpClient { get; }

        public abstract void SendRequest(IHttpRequest request, Action<IHttpResponse> response);

        public abstract void SendRequest(IHttpRequest request, Action<IHttpResponse, Error> response);

        public static HttpOperator CreateDefault(IHttpClient httpClient)
        {
            HttpOperator retval = null;
#if !UNITY_WEBGL
            retval = new HttpAsyncOperator(httpClient);
#else
            retval = new HttpCoroutineOperator(httpClient, new CoroutineRunner());
#endif
            return retval;
        }
    }

    public class HttpAsyncOperator : HttpOperator
    {
        readonly IHttpClient httpClient;
        public override IHttpClient HttpClient => httpClient;

        public HttpAsyncOperator(IHttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public override async void SendRequest(IHttpRequest request, Action<IHttpResponse> response)
        {
            var result = await httpClient.SendRequestAsync(request);
            response?.Invoke(result.CallbackResponse);
        }

        public override async void SendRequest(IHttpRequest request, Action<IHttpResponse, Error> response)
        {
            var result = await httpClient.SendRequestAsync(request);
            response?.Invoke(result.CallbackResponse, result.CallbackError);
        }
    }

    public class HttpCoroutineOperator : HttpOperator
    {
        readonly IHttpClient httpClient;
        readonly CoroutineRunner runner;

        public override IHttpClient HttpClient => httpClient;

        public HttpCoroutineOperator(IHttpClient httpClient, CoroutineRunner runner)
        {
            this.httpClient = httpClient;
            this.runner = runner;
        }

        public override void SendRequest(IHttpRequest request, Action<IHttpResponse> response)
        {
            runner.Run(httpClient.SendRequest(request, response));
        }

        public override void SendRequest(IHttpRequest request, Action<IHttpResponse, Error> response)
        {
            runner.Run(httpClient.SendRequest(request, response));
        }
    }
}