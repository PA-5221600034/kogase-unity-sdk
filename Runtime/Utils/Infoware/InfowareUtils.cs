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

    internal class OtherOs : InfowareUtils
    {
        internal override string GetDeviceUniqueIdentifier()
        {
            var uniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
            if (string.IsNullOrEmpty(uniqueIdentifier)) throw new Exception("Unable to retrieve device id");
            return uniqueIdentifier;
        }

        internal override string GetMacAddress()
        {
            return IdentifierProvider.GetDeviceMacAddress();
        }
    }
}