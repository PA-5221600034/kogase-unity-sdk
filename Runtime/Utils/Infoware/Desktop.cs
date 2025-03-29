using Kogase.Core;
using System;
using UnityEngine;

namespace Kogase.Utils
{
    internal class Windows : Infoware
    {
        internal override string GetDeviceUniqueIdentifier()
        {
            var uniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
            if (string.IsNullOrEmpty(uniqueIdentifier)) uniqueIdentifier = Guid.NewGuid().ToString();

            return uniqueIdentifier;
        }

        internal override string GetMacAddress()
        {
            return IdentifierProvider.GetDeviceMacAddress();
        }
    }

    internal class LinuxOS : Infoware
    {
        internal override string GetDeviceUniqueIdentifier()
        {
            var uniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
            if (string.IsNullOrEmpty(uniqueIdentifier)) uniqueIdentifier = Guid.NewGuid().ToString();

            return uniqueIdentifier;
        }

        internal override string GetMacAddress()
        {
            return IdentifierProvider.GetDeviceMacAddress();
        }
    }

    internal class MacOS : Infoware
    {
        internal override string GetDeviceUniqueIdentifier()
        {
            var uniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
            if (string.IsNullOrEmpty(uniqueIdentifier)) uniqueIdentifier = Guid.NewGuid().ToString();

            return uniqueIdentifier;
        }

        internal override string GetMacAddress()
        {
            return IdentifierProvider.GetDeviceMacAddress();
        }
    }
}