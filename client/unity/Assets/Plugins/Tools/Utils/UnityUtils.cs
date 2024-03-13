using System.Collections;
using UnityEngine;

namespace Wtf
{
    public static class UnityUtils
    {
        public static T InstantiateGameObjectWithComponent<T>(Transform parent) where T : Component
        {
            GameObject gameObject = new GameObject(typeof(T).ToString());
            gameObject.transform.SetParent(parent, false);
            gameObject.transform.localPosition = Vector3.zero;
            return gameObject.AddComponent<T>();
        }

        public static Coroutine StartCoroutine(MonoBehaviour owner, IEnumerator ie)
        {
            return owner.StartCoroutine(ie);
        }

        public static void StopCoroutine(MonoBehaviour owner, ref Coroutine coroutine)
        {
            if (coroutine != null)
            {
                owner.StopCoroutine(coroutine);
                coroutine = null;
            }
        }

        public static bool CoroutineRunning(ref Coroutine coroutine)
        {
            return coroutine != null;
        }

        public static void SetEnabledStateIfDifferent(MonoBehaviour mb, bool enabled)
        {
            if (mb.enabled != enabled)
            {
                mb.enabled = enabled;
            }
        }

        public static void SetActiveStateIfDifferent(GameObject go, bool active)
        {
            if (go.activeSelf != active)
            {
                go.SetActive(active);
            }
        }
    }
}