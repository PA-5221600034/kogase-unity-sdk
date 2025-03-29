#if UNITY_2021_3_OR_NEWER
using UnityEngine.Device;
#else
using UnityEngine;
#endif

namespace Kogase.Utils
{
    public static class CommonInfo
    {
        public static string PlatformName = Application.platform.ToString();
        public static string PlatformVersion = SystemInfo.operatingSystem.ToString();

        public static string AppName = Application.productName.ToString();
        public static string AppVersion = Application.version.ToString();

        public static string DeviceType = SystemInfo.deviceType.ToString();

#if !UNITY_SWITCH || UNITY_EDITOR
        static readonly string UnityPersistentPath = Application.persistentDataPath;
        public static string PersistentPath
        {
            get
            {
                var persistentPath = UnityPersistentPath;
                if (string.IsNullOrEmpty(persistentPath)) persistentPath = ".";

                return persistentPath;
            }
        }
#endif
    }
}