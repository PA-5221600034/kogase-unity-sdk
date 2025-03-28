// Copyright (c) 2023 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;

namespace Kogase.Core
{
    public interface ILogger
    {
        public void InvokeLog(LogType logType, object message, UnityEngine.Object context=null);
        public void InvokeException(Exception exception, UnityEngine.Object context=null);
    }
}
