namespace Kogase.Core
{
    internal interface IHttpRequestSenderFactory
    {
        IHttpRequestSender CreateHttpRequestSender();
        void ResetHttpRequestSenders();
    }
}