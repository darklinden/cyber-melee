using System;
using System.Collections;
using UnityEngine;

namespace Wtf
{
    public static class MonoBehaviourExtensions
    {
        private static IEnumerator DoSomethingAfterDelay(float afterSec, Action sth)
        {
            yield return Yielders.WaitForSeconds(afterSec);

            sth?.Invoke();
        }

        private static IEnumerator DoSomethingAfterRealDelay(float afterSec, Action sth)
        {
            yield return Yielders.WaitForSecondsRealtime(afterSec);

            sth?.Invoke();
        }

        private static IEnumerator DoSomethingOnNextFrame(Action sth)
        {
            yield return null;

            sth?.Invoke();
        }

        public static void DelayDo(this MonoBehaviour @this, float afterSec, Action sth)
        {
            @this.StartCoroutine(DoSomethingAfterDelay(afterSec, sth));
        }

        public static void DelayDo(this MonoBehaviour @this, Action sth)
        {
            @this.StartCoroutine(DoSomethingOnNextFrame(sth));
        }

        public static void DelayDoRealtime(this MonoBehaviour @this, float afterSec, Action sth)
        {
            @this.StartCoroutine(DoSomethingAfterRealDelay(afterSec, sth));
        }
    }
}