using UnityEngine;
namespace Kogase.Utils
{
    internal static class KogaseGameObject
    {
        static GameObject _sdkGameObject;

        const string SDKGameObjectName = "KogaseDummyGameObject";

        internal static GameObject GetOrCreateGameObject()
        {
            if(_sdkGameObject == null)
            {
                _sdkGameObject = GameObject.Find(SDKGameObjectName);
            }

            if (_sdkGameObject == null)
            {
                _sdkGameObject = new GameObject(SDKGameObjectName);
            }

            return _sdkGameObject;
        }
    }
}
