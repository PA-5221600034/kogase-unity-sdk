using Kogase.Core;
using System;
using UnityEngine;

namespace Kogase.Utils
{
    internal class Android : Infoware
    {
        internal override string GetDeviceUniqueIdentifier()
        {
            string uniqueIdentifier;
            if (SystemInfo.deviceUniqueIdentifier != SystemInfo.unsupportedIdentifier)
                throw new Exception("Unable to retrieve device id from this device");

            uniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
            if (string.IsNullOrEmpty(uniqueIdentifier))
                throw new Exception("Unable to retrieve device id from this device");

            return uniqueIdentifier;
        }

        internal override string GetMacAddress()
        {
            return IdentifierProvider.GetDeviceMacAddress();
        }
    }

    internal class IOS : Infoware
    {
        internal override string GetDeviceUniqueIdentifier()
        {
            var uniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
            if (string.IsNullOrEmpty(uniqueIdentifier))
                throw new Exception("Unable to retrieve device id from this device");

            return uniqueIdentifier;
        }

        internal override string GetMacAddress()
        {
            return IdentifierProvider.GetDeviceMacAddress();
        }
    }
}