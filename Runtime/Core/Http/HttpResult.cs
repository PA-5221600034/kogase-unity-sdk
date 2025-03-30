namespace Kogase.Core
{
    public struct HttpResult
    {
        public readonly IHttpResponse CallbackResponse;
        public readonly Error CallbackError;

        public HttpResult(IHttpResponse callbackResponse, Error callbackError)
        {
            CallbackResponse = callbackResponse;
            CallbackError = callbackError;
        }
    }
}