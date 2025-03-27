using System;
using System.Collections.Generic;
using System.IO;
using Kogase.Core;
using UnityEngine;

namespace Kogase
{
    public class KogaseSDK
    {
        static KogaseSDK _instance;
        public static KogaseSDK Instance
        {
            get { return _instance ??= new KogaseSDK(); }
        }

        Queue<Dictionary<string, object>> eventQueue;

        KogaseSDK()
        {
            eventQueue = new Queue<Dictionary<string, object>>();
        }
    }
} 