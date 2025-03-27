using System;
using System.Security.Cryptography;
using UnityEngine.Device;
namespace Kogase.Core
{
    public static class MachineCode
    {
        const int KDeviceUniqueIdentifierMaxLength = 8;

        /// <summary>
        /// Generates a new secure machine code with platform enhancements
        /// </summary>
        static string GenerateMachineCode()
        {
            var guidPart = Guid.NewGuid().ToString("N");
            
            var platformPart = GetPlatformSpecificId();
            
            // Combine with cryptographic 
            using (var sha256 = SHA256.Create())
            {
                var input = $"{guidPart}-{platformPart}-{DateTime.UtcNow.Ticks}";
                var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        /// <summary>
        /// Gets platform-specific identifier when available
        /// </summary>
        static string GetPlatformSpecificId()
        {
            try
            {
#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
                if (!string.IsNullOrEmpty(SystemInfo.deviceUniqueIdentifier))
                {
                    return SystemInfo.deviceUniqueIdentifier
                        .Substring(
                            0, 
                            Math.Min(
                                KDeviceUniqueIdentifierMaxLength, 
                                SystemInfo.deviceUniqueIdentifier.Length
                            )
                        );
                }
#endif
                
                return Environment.MachineName.GetHashCode().ToString("X8");
            }
            catch
            {
                return "00000000"; // Fallback if any errors occur
            }
        }
    }
}