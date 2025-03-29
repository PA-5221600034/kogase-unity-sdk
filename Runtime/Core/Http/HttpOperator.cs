using System;
using System.Collections;
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
            HttpOperator retval;
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
        private readonly IHttpClient httpClient;
        
        public override IHttpClient HttpClient => httpClient;

        public HttpAsyncOperator(IHttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public override async void SendRequest(IHttpRequest request, Action<IHttpResponse> response)
        {
            try
            {
                HttpSendResult result = await httpClient.SendRequestAsync(request);
                response?.Invoke(result.CallbackResponse);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public override async void SendRequest(IHttpRequest request, Action<IHttpResponse, Error> response)
        {
            try
            {
                HttpSendResult result = await httpClient.SendRequestAsync(request);
                response?.Invoke(result.CallbackResponse, result.CallbackError);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    public class HttpCoroutineOperator : HttpOperator
    {
        private readonly IHttpClient httpClient;
        private readonly CoroutineRunner runner;

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