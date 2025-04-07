using Kogase.Api;
using System;

namespace Kogase
{
    /// <summary>
    /// Main entry point for the Kogase SDK
    /// </summary>
    public static class KogaseSDK
    {
        static KogaseSDKImpl _implementation;

        internal static KogaseSDKImpl Implementation
        {
            get => _implementation ??= new KogaseSDKImpl();
            set => _implementation = value;
        }

        public static string Version => Implementation.Version;

        public static CommonApi Api => Implementation.Api;

        public static void BeginSession(Action okCallback = null, Action errorCallback = null)
        {
            Implementation.SessionManager.BeginSession(okCallback, errorCallback);
        }
    }
}