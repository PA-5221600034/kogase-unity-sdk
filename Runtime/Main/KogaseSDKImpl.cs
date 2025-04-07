using Kogase.Api;
using Kogase.Core;

namespace Kogase
{
    internal class KogaseSDKImpl
    {
        CommonApi _api;

        internal CommonApi Api
        {
            get
            {
                if (_api != null) return _api;
                _api = new CommonApi(new KogaseHttpClient(), KogaseSettings.SDKConfig);

                return _api;
            }
        }

        HeartBeat _heartBeat;
        HeartBeat HeartBeat => _heartBeat ??= new HeartBeat();

        public string Version => KogaseSettings.SDKVersion;

        SessionManager _sessionManager;

        internal SessionManager SessionManager
        {
            get
            {
                if (_sessionManager != null) return _sessionManager;
                _sessionManager = new SessionManager();
                return _sessionManager;
            }
        }

        EventManager _eventManager;

        internal EventManager EventManager
        {
            get
            {
                if (_eventManager != null) return _eventManager;
                _eventManager = new EventManager(new EventCache(KogaseSettings.SDKConfig, FileStream));
                return _eventManager;
            }
        }

        public KogaseSDKImpl()
        {
        }

        internal string GetOrCreateDeviceIdentifier()
        {
            return IdentifierProvider.GetFromSystemInfo(KogaseSettings.SDKConfig.ApiKey, fs: FileStream).Identifier;
        }

        internal void Reset()
        {
            if (SessionManager is { IsSessionActive: true })
                SessionManager.EndSession();

            if (_heartBeat != null)
            {
                _heartBeat.Reset();
                _heartBeat = null;
            }

            EventManager.SavePendingEvents();

            _api = null;
            _sessionManager = null;
            _eventManager = null;
        }

        #region File Stream

        IFileStream _fileStream;
        internal IFileStream FileStream => _fileStream ??= CreateFileStream();

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
            if (_fileStream != null) _fileStream.Dispose();
        }

        #endregion File Stream
    }
}