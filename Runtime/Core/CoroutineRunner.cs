using System;
using System.Collections;
using System.Collections.Generic;
using Kogase.Utils;
using UnityEngine;

namespace Kogase.Core
{
    public class CoroutineRunner
    {
        readonly MonoBehaviour monoBehaviour;
        readonly Queue<Action> callbacks = new();
        readonly object syncToken = new();

        bool isRunning = true;

        public CoroutineRunner()
        {
            var sdkGameObject = KogaseGameObject.GetOrCreateGameObject();

            monoBehaviour = sdkGameObject.GetComponent<DummyBehaviour>();
            if (monoBehaviour == null) monoBehaviour = sdkGameObject.AddComponent<DummyBehaviour>();

            monoBehaviour.StartCoroutine(RunCallbacks());
        }

        ~CoroutineRunner()
        {
            isRunning = false;
        }

        public Coroutine Run(IEnumerator coroutine)
        {
            return monoBehaviour != null ? monoBehaviour.StartCoroutine(coroutine) : null;
        }

        public void Stop(Coroutine coroutine)
        {
            monoBehaviour.StopCoroutine(coroutine);
        }

        public void Run(Action callback)
        {
            lock (syncToken)
            {
                callbacks.Enqueue(callback);
            }
        }

        IEnumerator RunCallbacks()
        {
            while (isRunning)
            {
                yield return new WaitUntil(() => callbacks.Count > 0);

                Action callback;

                lock (syncToken)
                {
                    callback = callbacks.Dequeue();
                }

                callback();
            }
        }
    }
}