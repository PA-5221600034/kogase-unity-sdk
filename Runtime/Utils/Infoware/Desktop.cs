using Kogase.Core;
using System;
using UnityEngine;

namespace Kogase.Utils.Infoware
{
    internal class Windows : InfowareUtils
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

    internal class LinuxOS : InfowareUtils
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

    internal class MacOS : InfowareUtils
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