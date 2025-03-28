using UnityEngine;

namespace Kogase.Core
{
    public class DummyBehaviour : MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}