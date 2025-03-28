namespace Kogase.Bootstrap
{
    internal static class SdkInterfaceBootstrap
    {
        internal static void Execute()
        {
        }

        public static void Stop()
        {
            KogaseSDK.Implementation.Reset();
        }
    }
}
