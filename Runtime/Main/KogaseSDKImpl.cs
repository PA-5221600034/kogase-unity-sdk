using Kogase.Api;
using Kogase.Core;
using Kogase.Dtos;
using Kogase.Models;
using Kogase.Utils;

namespace Kogase
{
    internal class KogaseSDKImpl
    {
        CommonApi api;
        internal CommonApi Api
        {
            get
            {
                if (api != null) return api;
                var config = KogaseSettings.SDKConfig;
                api = new CommonApi(new KogaseHttpClient(), config);

                return api;
            }
        }

        HeartBeat heartBeat;
        HeartBeat HeartBeat => heartBeat ??= new HeartBeat();

        public string Version => KogaseSettings.SDKVersion;

        readonly KogaseServiceTracker serviceTracker;

        public KogaseSDKImpl()
        {
            serviceTracker = new KogaseServiceTracker();
            var serviceLogger = new KogaseServiceLogger();

            serviceTracker.OnNewRequestSentEvent += serviceLogger.LogServiceActivity;
            serviceTracker.OnNewResponseReceivedEvent += serviceLogger.LogServiceActivity;
            serviceTracker.OnSendingWebsocketRequestEvent += serviceLogger.LogServiceActivity;
            serviceTracker.OnReceivingWebsocketNotificationEvent += serviceLogger.LogServiceActivity;
        }

        public KogaseConfig GetConfig()
        {
            var retval = KogaseSettings.SDKConfig;
            retval = retval.Clone();
            return retval;
        }

        internal void Reset()
        {
            // TODO: Implement reset logic

            if (heartBeat != null)
            {
                heartBeat.Reset();
                heartBeat = null;
            }
        }

        #region File Stream

        IFileStream fileStream;
        internal IFileStream FileStream => fileStream ??= CreateFileStream();
        
        public IFileStreamFactory FileStreamFactory;

        IFileStream CreateFileStream()
        {
            var fileStreamFactory = FileStreamFactory ?? new FileStreamFactory();

            var createdFileStream = fileStreamFactory.CreateFileStream();

            KogaseSDKMain.OnUpdate += dt => { createdFileStream.Pop(); };

            return createdFileStream;
        }

        internal void DisposeFileStream()
        {
            if (fileStream != null) fileStream.Dispose();
        }

        #endregion File Stream
    }
}