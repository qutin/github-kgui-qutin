using System;
using System.Collections;
using UnityEngine;

namespace KGUI
{
    public static class CoroutineRunner
    {
        private static CoroutineBehavior _coroutineBehavior;
        private class CoroutineBehavior : MonoBehaviour
        {
            void Start()
            {
                Application.runInBackground = true;
                DontDestroyOnLoad(this.gameObject);

            }
        }

        public static void StartCoroutine(IEnumerator iterator)
        {
            if (_coroutineBehavior == null)
            {
                _coroutineBehavior = new GameObject("couroutineRunner").AddComponent<CoroutineBehavior>();                
            }
            _coroutineBehavior.StartCoroutine(iterator);
        }
    }
}
