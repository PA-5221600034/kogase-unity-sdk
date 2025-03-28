namespace Kogase.Core
{
    internal class HttpRequestFactory : IHttpRequestSenderFactory
    {
        public System.Action<WebRequestScheduler> OnWebRequestSchedulerCreated;
        readonly HeartBeat heartBeat;
        readonly System.Collections.Generic.List<IHttpRequestSender> createdHttpRequestSenderRecord;

        public HttpRequestFactory(HeartBeat hb)
        {
            heartBeat = hb;
            createdHttpRequestSenderRecord = new System.Collections.Generic.List<IHttpRequestSender>();
        }

        public IHttpRequestSender CreateHttpRequestSender()
        {
            var httpSenderScheduler = new WebRequestScheduler();
            var sdkHttpSender = new HttpRequestSender(httpSenderScheduler);
            sdkHttpSender.SetHeartBeat(heartBeat);
            createdHttpRequestSenderRecord.Add(sdkHttpSender);
            OnWebRequestSchedulerCreated?.Invoke(httpSenderScheduler);

            return sdkHttpSender;
        }

        public void ResetHttpRequestSenders()
        {
            foreach (var httpRequestSender in createdHttpRequestSenderRecord) httpRequestSender.ClearTasks();
        }
    }
}