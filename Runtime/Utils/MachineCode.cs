using Komutil.JsonPlayerPrefs;
using System;
using System.Security.Cryptography;
using UnityEngine;
namespace Kogase.Utils
{
    /// <summary>
    /// Secure machine code generator with platform-specific optimizations
    /// </summary>
    public static class MachineCode
    {
        const string KMachineCode = "kogase_machine_code";
        const int KDeviceUniqueIdentifierMaxLength = 8;
        
        /// <summary>
        /// Gets or creates a persistent machine code with platform optimizations
        /// </summary>
        public static string GetOrCreateMachineCode()
        {
            if (JsonPlayerPrefs.HasKey(KMachineCode))
            {
                var existingCode = JsonPlayerPrefs.GetString(KMachineCode);
                if (!string.IsNullOrEmpty(existingCode))
                {
                    return existingCode;
                }
            }

            var newCode = GenerateMachineCode();
            JsonPlayerPrefs.SetString(KMachineCode, newCode);
            JsonPlayerPrefs.Save();
            
            return newCode;
        }

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

        /// <summary>
        /// Resets the machine code (use carefully - creates a new identity)
        /// </summary>
        public static void ResetMachineCode()
        {
            JsonPlayerPrefs.DeleteKey(KMachineCode);
            JsonPlayerPrefs.Save();
        }
    }
} 