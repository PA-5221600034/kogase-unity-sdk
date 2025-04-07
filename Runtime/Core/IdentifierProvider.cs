using Kogase.Utils;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.Assertions;
using UnityEngine;

namespace Kogase.Core
{
    public class IdentifierProvider
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
        static readonly string CacheFileName = "DeviceIdentifier";

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

        public IdentifierProvider(string type, string identifier, string uniqueId)
            : this(type, identifier, uniqueId, false)
        {
        }

        internal IdentifierProvider(string type, string identifier, string uniqueId, bool isGenerated)
        {
            Assert.IsNotNull(identifier, "Identifier is null!");
            Assert.IsNotNull(type, "Type is null!");
            Assert.IsNotNull(uniqueId, "Unique ID is null!");
            Identifier = $"{identifier}_{uniqueId}";
            Type = type;
            UniqueId = uniqueId;
            IsGenerated = isGenerated;
        }

        public static IdentifierProvider GetFromSystemInfo(
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
            var randomizeUniqueIdentifier = identifierGeneratorConfig is { RandomizeIdentifier: true };
            var platformUniqueIdentifier = !randomizeUniqueIdentifier
                ? GetIdentifier(encodeKey)
                : null;
            var isIdentifierGenerated = string.IsNullOrEmpty(platformUniqueIdentifier);
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
                        Debug.Log($"Retrieve cached identifier: {platformUniqueIdentifier}");
                        // logger?.LogVerbose($"Retrieve cached identifier: {platformUniqueIdentifier}");
                    }
                    else
                    {
                        platformUniqueIdentifier = GenerateIdentifier();
                        fileCache.Emplace(CacheFileName, platformUniqueIdentifier);
                        Debug.Log($"Generate new identifier: {platformUniqueIdentifier}");
                        // logger?.LogVerbose($"Generate new identifier: {platformUniqueIdentifier}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Unable to access identifier cache, {e.Message}");
                    // logger?.LogWarning($"Unable to access identifier cache, {exception.Message}");

                    if (string.IsNullOrEmpty(platformUniqueIdentifier))
                        platformUniqueIdentifier = GenerateIdentifier();

                    Debug.LogWarning($"Generate new identifier: {platformUniqueIdentifier}");
                    // logger?.LogVerbose($"Generate new identifier: {platformUniqueIdentifier}");
                }

            var identifier = $"unity_{CommonInfo.DeviceType}_{CommonInfo.PlatformName}";

            var retval = new IdentifierProvider(
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
                var fileCache =
                    new FileCacheImpl(cacheFileDir, fs);
                fileCache.Emplace(CacheFileName, identifier);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Unable to access identifier cache, {e.Message}");
                // logger?.LogWarning($"Unable to cache identifier, {exception.Message}");
            }
        }

        static string GetIdentifier(string encodeKey)
        {
            Infoware inforware;
            string platformUniqueIdentifier;

            try
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.OSXEditor:
                    case RuntimePlatform.OSXPlayer:
                    {
                        inforware = new MacOS();
                        var macAddress = inforware.GetMacAddress();
                        platformUniqueIdentifier = EncodeHmac(macAddress, encodeKey);
                        break;
                    }
                    case RuntimePlatform.WindowsEditor:
                    case RuntimePlatform.WindowsPlayer:
                    {
                        inforware = new Windows();
                        var macAddress = inforware.GetMacAddress();
                        platformUniqueIdentifier = EncodeHmac(macAddress, encodeKey);
                        break;
                    }
                    case RuntimePlatform.LinuxEditor:
                    case RuntimePlatform.LinuxPlayer:
                    {
                        inforware = new LinuxOS();
                        var macAddress = inforware.GetMacAddress();
                        platformUniqueIdentifier = EncodeHmac(macAddress, encodeKey);
                        break;
                    }
                    case RuntimePlatform.IPhonePlayer:
                    {
                        inforware = new IOS();
                        var deviceId = inforware.GetDeviceUniqueIdentifier();
                        platformUniqueIdentifier = EncodeHmac(deviceId, encodeKey);
                        break;
                    }
                    case RuntimePlatform.Android:
                    {
                        inforware = new Android();
                        var deviceId = inforware.GetDeviceUniqueIdentifier();
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
                        inforware = new OtherOs();
                        var uniqueIdentifier = inforware.GetMacAddress();
                        if (string.IsNullOrEmpty(uniqueIdentifier))
                            uniqueIdentifier = inforware.GetDeviceUniqueIdentifier();
                        platformUniqueIdentifier = EncodeHmac(uniqueIdentifier, encodeKey);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Unable to get identifier, {e.Message}");
                // logger?.LogVerbose(e.Message);
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

        static string GenerateIdentifier()
        {
            var newGuid = Guid.NewGuid().ToString();
            return newGuid;
        }
    }
}