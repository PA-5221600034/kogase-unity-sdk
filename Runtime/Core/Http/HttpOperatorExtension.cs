namespace Kogase.Core
{
    internal static class HttpOperatorExtension
    {
        /// <summary>
        /// Send a custom request
        /// </summary>
        public static void Send<TResponse>(this HttpOperator httpOperator, IHttpRequest request,
            OkDelegate<TResponse> okCallback, ErrorDelegate<Error> errorCallback)
        {
            httpOperator.SendRequest(request, (response, error) =>
            {
                if (error != null)
                {
                    errorCallback?.Invoke(error);
                    return;
                }

                HttpParser.ParseResponse(response, okCallback, errorCallback);
            });
        }
    }
}