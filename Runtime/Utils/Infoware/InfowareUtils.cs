using Kogase.Core;
using System;
using UnityEngine;
namespace Kogase.Utils.Infoware
{
    public abstract class InfowareUtils
    {
        internal abstract string GetDeviceUniqueIdentifier();
        internal abstract string GetMacAddress();
    }

    class OtherOs : InfowareUtils
    {
        internal override string GetDeviceUniqueIdentifier()
        {
            string uniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
            if(string.IsNullOrEmpty(uniqueIdentifier))
            {
                throw new Exception("Unable to retrieve device id");
            }
            return uniqueIdentifier;
        }

        internal override string GetMacAddress()
        {
            return DeviceProvider.GetDeviceMacAddress();
        }
    }
}
