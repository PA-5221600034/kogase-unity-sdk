using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace Kogase.Core
{
    public class DeviceProvider
    {
        public readonly string Type;
        public readonly string Identifier;
        public readonly string UniqueId;
        internal readonly bool IsGenerated;

#if UNITY_SWITCH && !UNITY_EDITOR
        static readonly string DefaultCacheFileDir = "Kogase/";
#else
        static readonly string DefaultCacheFileDir = $"{CommonInfo.PersistentPath}/Kogase/";
#endif
        static readonly string CacheFileName = "MachineIdentifier";

        static readonly char[] EligibleCharacters =
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'g',
            'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'
        };

        static System.Random _randomizer;
        static int _seed;

        static string EncodeHmac(string macAddress, string key)
        {
            if (string.IsNullOrEmpty(macAddress)) return macAddress;

            try
            {
                var byteArray = Encoding.ASCII.GetBytes(macAddress);
                using (var myhmacsha1 = new HMACSHA1(Encoding.ASCII.GetBytes(key)))
                {
                    var hashArray = myhmacsha1.ComputeHash(byteArray);
                    return hashArray.Aggregate("", (s, e) => $"{s}{e:x2}", s => s);
                }
            }
            catch (InvalidOperationException e)
            {
                throw e;
            }
        }

        public DeviceProvider(string type, string identifier, string uniqueId) 
            : this(type, identifier, uniqueId, false)
        {
        }

        internal DeviceProvider(string type, string identifier, string uniqueId, bool isGenerated)
        {
            Assert.IsNotNull(identifier, "Identifier is null!");
            Assert.IsNotNull(type, "Type is null!");
            Assert.IsNotNull(uniqueId, "Unique ID is null!");
            Identifier = $"{identifier}_{uniqueId}";
            Type = type;
            UniqueId = uniqueId;
            IsGenerated = isGenerated;
        }

        public static DeviceProvider GetFromSystemInfo(
            string encodeKey, 
            string generatedIdCacheFileDir = null,
            IFileStream fs = null,
            Models.IdentifierGeneratorConfig identifierGeneratorConfig = null
        )
        {
#if UNITY_WEBGL
            string platformUniqueIdentifier = null;
            bool isIdentifierGenerated = true;
#else
            bool randomizeUniqueIdentifier = identifierGeneratorConfig is { RandomizeIdentifier: true };
            string platformUniqueIdentifier = !randomizeUniqueIdentifier 
                ? GetIdentifier(encodeKey) 
                : null;
            bool isIdentifierGenerated = string.IsNullOrEmpty(platformUniqueIdentifier);
#endif

            if (isIdentifierGenerated)
                try
                {
                    if (string.IsNullOrEmpty(generatedIdCacheFileDir))
                        generatedIdCacheFileDir = DefaultCacheFileDir;

                    var fileCache = new FileCacheImpl(generatedIdCacheFileDir, fs);
                    if (fileCache.Contains(CacheFileName))
                    {
                        platformUniqueIdentifier = fileCache.Retrieve(CacheFileName);
                        // logger?.LogVerbose($"Retrieve cached device id: {platformUniqueIdentifier}");
                    }
                    else
                    {
                        platformUniqueIdentifier = GenerateIdentifier();
                        fileCache.Emplace(CacheFileName, platformUniqueIdentifier);
                        // logger?.LogVerbose($"Generate new device id: {platformUniqueIdentifier}");
                    }
                }
                catch (Exception exception)
                {
                    // logger?.LogWarning($"Unable to access device id cache, {exception.Message}");

                    if (string.IsNullOrEmpty(platformUniqueIdentifier)) 
                        platformUniqueIdentifier = GenerateIdentifier();
                    
                    // logger?.LogVerbose($"Generate new device id: {platformUniqueIdentifier}");
                }

            string identifier = $"unity_{CommonInfo.DeviceType}_{GetPlatformName()}";

            DeviceProvider retval = new DeviceProvider(
                "device",
                identifier,
                platformUniqueIdentifier,
                isIdentifierGenerated
            );
            
            return retval;
        }

        public static void CacheIdentifier(string identifier, string cacheFileDir = null, IFileStream fs = null)
        {
            try
            {
                if (string.IsNullOrEmpty(cacheFileDir))
                    cacheFileDir = DefaultCacheFileDir;
                FileCacheImpl fileCache =
                    new FileCacheImpl(cacheFileDir, fs);
                fileCache.Emplace(CacheFileName, identifier);
            }
            catch (Exception exception)
            {
                // logger?.LogWarning($"Unable to cache device id, {exception.Message}");
            }
        }

        static string GetIdentifier(string encodeKey)
        {
            Utils.Infoware.InfowareUtils iware;
            string platformUniqueIdentifier;

            try
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.OSXEditor:
                    case RuntimePlatform.OSXPlayer:
                    {
                        iware = new Utils.Infoware.MacOS();
                        string macAddress = iware.GetMacAddress();
                        platformUniqueIdentifier = EncodeHmac(macAddress, encodeKey);
                        break;
                    }
                    case RuntimePlatform.WindowsEditor:
                    case RuntimePlatform.WindowsPlayer:
                    {
                        iware = new Utils.Infoware.Windows();
                        string macAddress = iware.GetMacAddress();
                        platformUniqueIdentifier = EncodeHmac(macAddress, encodeKey);
                        break;
                    }
                    case RuntimePlatform.LinuxEditor:
                    case RuntimePlatform.LinuxPlayer:
                    {
                        iware = new Utils.Infoware.LinuxOS();
                        string macAddress = iware.GetMacAddress();
                        platformUniqueIdentifier = EncodeHmac(macAddress, encodeKey);
                        break;
                    }
                    case RuntimePlatform.IPhonePlayer:
                    {
                        iware = new Utils.Infoware.IOS();
                        string deviceId = iware.GetDeviceUniqueIdentifier();
                        platformUniqueIdentifier = EncodeHmac(deviceId, encodeKey);
                        break;
                    }
                    case RuntimePlatform.Android:
                    {
                        iware = new Utils.Infoware.Android();
                        string deviceId = iware.GetDeviceUniqueIdentifier();
                        platformUniqueIdentifier = EncodeHmac(deviceId, encodeKey);
                        break;
                    }
                    case RuntimePlatform.WebGLPlayer:
                    {
                        string newGuid;
                        if (!PlayerPrefs.HasKey("AccelByteDeviceUniqueId"))
                        {
                            newGuid = GenerateIdentifier();
                            PlayerPrefs.SetString("AccelByteDeviceUniqueId", newGuid);
                        }
                        else
                        {
                            newGuid = PlayerPrefs.GetString("AccelByteDeviceUniqueId");
                        }

                        platformUniqueIdentifier = newGuid;
                        break;
                    }
                    default:
                    {
                        iware = new Utils.Infoware.OtherOs();
                        string uniqueIdentifier = iware.GetMacAddress();
                        if (string.IsNullOrEmpty(uniqueIdentifier)) uniqueIdentifier = iware.GetDeviceUniqueIdentifier();
                        platformUniqueIdentifier = EncodeHmac(uniqueIdentifier, encodeKey);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                // logger?.LogVerbose(ex.Message);
                platformUniqueIdentifier = null;
            }

            return platformUniqueIdentifier;
        }

        public static string GetDeviceMacAddress()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up)
                .Select(nic => nic.GetPhysicalAddress().ToString())
                .FirstOrDefault();
        }

        public static string[] GetMacAddress()
        {
            string[] macAddressArray = null;
#if (!UNITY_SWITCH && !UNITY_WEBGL && !UNITY_IOS && !UNITY_ANDROID) || UNITY_EDITOR
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            macAddressArray = new string[] { };
            foreach (var adapter in networkInterfaces)
            {
                var physicalAddress = adapter.GetPhysicalAddress().ToString();
                if (!string.IsNullOrEmpty(physicalAddress))
                    macAddressArray = macAddressArray.Concat(new string[] { physicalAddress }).ToArray();
            }
#endif
            return macAddressArray;
        }

        internal static string GetPlatformName()
        {
            return CommonInfo.PlatformName;
        }

        static string GenerateIdentifier()
        {
            var newGuid = Guid.NewGuid().ToString();
            return newGuid;
        }
    }
}