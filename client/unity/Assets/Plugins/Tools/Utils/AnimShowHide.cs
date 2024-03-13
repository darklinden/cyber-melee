using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Wtf
{
    public class AnimShowHide : MonoBehaviour
    {
        public CanvasGroup AnimTarget = null;
        public float AnimTime = 0.2f;

        Coroutine _coroutineShow = null;
        private bool StopAnimShow()
        {
            bool result = false;
            if (_coroutineShow != null)
            {
                result = true;
                StopCoroutine(_coroutineShow);
                _coroutineShow = null;
            }
            return result;
        }

        Coroutine _coroutineHide = null;
        private bool StopAnimHide()
        {
            var result = false;
            if (_coroutineHide != null)
            {
                result = true;
                StopCoroutine(_coroutineHide);
                _coroutineHide = null;
            }
            return result;
        }

        public void Hide()
        {
            StopAnimShow();
            StopAnimHide();
            AnimTarget.alpha = 0f;
        }

        public void AnimShow(Action onComplete = null)
        {
            // Log.D("AnimShowHide.AnimShow");
            StopAnimHide();
            var preRunningShowStopped = StopAnimShow();
            if (preRunningShowStopped)
            {
                Log.W("AnimShowHide.AnimShow preRunningShowStopped");
                AnimTarget.alpha = 1f;
                onComplete?.Invoke();
            }
            else
            {
                _coroutineShow = StartCoroutine(AnimShowCoroutine(onComplete));
            }
        }

        public async UniTask AsyncAnimShow()
        {
            // Log.D("AnimShowHide.AnimShow");
            StopAnimHide();
            var preRunningShowStopped = StopAnimShow();
            if (preRunningShowStopped)
            {
                Log.W("AnimShowHide.AnimShow preRunningShowStopped");
                AnimTarget.alpha = 1f;
            }
            else
            {
                await UniTask.Yield();
                var completionSource = AutoResetUniTaskCompletionSource.Create();
                _coroutineShow = StartCoroutine(AnimShowCoroutine(() =>
                {
                    completionSource.TrySetResult();
                }));

                await completionSource.Task;
            }
        }

        private IEnumerator AnimShowCoroutine(Action onComplete)
        {
            var time = 0f;
            while (time < AnimTime)
            {
                AnimTarget.alpha = Mathf.Lerp(0f, 1f, time / AnimTime);
                time += Time.unscaledDeltaTime;
                yield return null;
            }
            AnimTarget.alpha = 1f;
            _coroutineShow = null;
            onComplete?.Invoke();
        }

        public void AnimHide(Action onComplete = null)
        {
            // Log.D("AnimShowHide.AnimHide");
            StopAnimShow();
            var preRunningHideStopped = StopAnimHide();
            if (preRunningHideStopped)
            {
                Log.W("AnimShowHide.AnimHide preRunningHideStopped");
                AnimTarget.alpha = 0f;
                onComplete?.Invoke();
            }
            else
            {
                _coroutineHide = StartCoroutine(AnimHideCoroutine(onComplete));
            }
        }

        public async UniTask AsyncAnimHide()
        {
            // Log.D("AnimShowHide.AnimHide");
            StopAnimShow();
            var preRunningHideStopped = StopAnimHide();
            if (preRunningHideStopped)
            {
                Log.W("AnimShowHide.AnimHide preRunningHideStopped");
                AnimTarget.alpha = 0f;
            }
            else
            {
                await UniTask.Yield();
                var completionSource = AutoResetUniTaskCompletionSource.Create();
                _coroutineHide = StartCoroutine(AnimHideCoroutine(() =>
                {
                    completionSource.TrySetResult();
                }));

                await completionSource.Task;
            }
        }

        private IEnumerator AnimHideCoroutine(Action onComplete)
        {
            var time = 0f;
            while (time < AnimTime)
            {
                AnimTarget.alpha = Mathf.Lerp(1f, 0f, time / AnimTime);
                time += Time.unscaledDeltaTime;
                yield return null;
            }
            AnimTarget.alpha = 0f;
            _coroutineHide = null;
            onComplete?.Invoke();
        }
    }
}
