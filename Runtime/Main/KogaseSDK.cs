using Kogase.Core;
using Kogase.Dtos;

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
        
        public static void TestConnection(OkDelegate<HealthResponse> okCallback = null, ErrorDelegate<Error> errorCallback = null)
        {
            Implementation.Api.TestConnection(okCallback, errorCallback);
        }
        
        public static void CreateProject(CreateProjectRequest payload, OkDelegate<CreateProjectResponse> okCallback = null, ErrorDelegate<Error> errorCallback = null)
        {
            Implementation.Api.CreateProject(payload, okCallback, errorCallback);
        }
    }
}