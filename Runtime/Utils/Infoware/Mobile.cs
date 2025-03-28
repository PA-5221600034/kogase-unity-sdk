using Kogase.Core;
using System;
using UnityEngine;

namespace Kogase.Utils.Infoware
{
    internal class Android : InfowareUtils
    {
        internal override string GetDeviceUniqueIdentifier()
        {
            string uniqueIdentifier;
            if (SystemInfo.deviceUniqueIdentifier != SystemInfo.unsupportedIdentifier)
            {
                throw new Exception("Unable to retrieve device id from this device");
            }

            uniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
            if (string.IsNullOrEmpty(uniqueIdentifier))
                throw new Exception("Unable to retrieve device id from this device");

            return uniqueIdentifier;
        }

        internal override string GetMacAddress()
        {
            return DeviceProvider.GetDeviceMacAddress();
        }
    }

    internal class IOS : InfowareUtils
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
            return DeviceProvider.GetDeviceMacAddress();
        }
    }
}