using System;
using UnityEngine;

namespace Kogase.Core
{
    [ExecuteInEditMode]
    public class MonoBehaviourSignaller : MonoBehaviour, IMonoBehaviourSignaller
    {
        public Action<float> OnUpdateSignal { get; set; }

        void Update()
        {
            OnUpdateSignal?.Invoke(Time.unscaledDeltaTime);
        }
    }
}