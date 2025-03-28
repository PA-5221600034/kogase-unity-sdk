namespace Kogase.Core
{
    public struct HttpSendResult
    {
        public readonly IHttpResponse CallbackResponse;
        public readonly Error CallbackError;

        public HttpSendResult(IHttpResponse callbackResponse, Error callbackError)
        {
            CallbackResponse = callbackResponse;
            CallbackError = callbackError;
        }
    }
}