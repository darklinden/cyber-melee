using System.Collections.Generic;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Wtf
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasGroupAlphaFading : MonoBehaviour
    {
        public CanvasGroup CanvasGroup;

        private Image m_image;

        private ManualTimer m_timer = new ManualTimer();

        private Coroutine m_animationRoutine;

        public bool IsAnimating
        {
            get
            {
                return UnityUtils.CoroutineRunning(ref m_animationRoutine);
            }
        }

        public bool IsTransparent
        {
            get
            {
                return CanvasGroup.alpha == 0f;
            }
        }

        public async UniTask animateToBlack(float duration)
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            if (m_animationRoutine != null)
            {
                StopCoroutine(m_animationRoutine);
                m_animationRoutine = null;
            }

            var animationCompletionSource = AutoResetUniTaskCompletionSource.Create();
            m_animationRoutine = StartCoroutine(animationRoutine(1f, duration, animationCompletionSource));
            try
            {
                await animationCompletionSource.Task;
            }
            catch (System.OperationCanceledException)
            {
                // ignore
            }
        }

        public async UniTask animateToTransparent(float duration)
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            if (m_animationRoutine != null)
            {
                StopCoroutine(m_animationRoutine);
                m_animationRoutine = null;
            }

            var animationCompletionSource = AutoResetUniTaskCompletionSource.Create();
            m_animationRoutine = StartCoroutine(animationRoutine(0f, duration, animationCompletionSource));
            try
            {
                await animationCompletionSource.Task;
            }
            catch (System.OperationCanceledException)
            {
                // ignore
            }
        }

        public void setTransparent(bool transparent)
        {
            if (m_animationRoutine != null)
            {
                StopCoroutine(m_animationRoutine);
                m_animationRoutine = null;
            }

            CanvasGroup.alpha = ((!transparent) ? 1f : 0f);
            if (m_image != null)
            {
                m_image.enabled = !transparent;
            }
        }

        private void Reset()
        {
            if (CanvasGroup == null)
            {
                CanvasGroup = GetComponent<CanvasGroup>();
            }
        }

        protected void Awake()
        {
            if (CanvasGroup == null)
            {
                CanvasGroup = GetComponent<CanvasGroup>();
            }
            m_image = GetComponent<Image>();
        }

        private IEnumerator animationRoutine(float targetAlpha, float duration, AutoResetUniTaskCompletionSource completionSource)
        {
            if (m_image != null)
            {
                m_image.enabled = true;
            }

            float fromAlpha = CanvasGroup.alpha;

            if (duration > 0f)
            {
                m_timer.set(duration);
                while (!m_timer.Idle)
                {
                    CanvasGroup.alpha = fromAlpha + (targetAlpha - fromAlpha) * m_timer.normalizedProgress();
                    m_timer.tick(Time.unscaledDeltaTime);
                    yield return null;
                }
            }

            CanvasGroup.alpha = targetAlpha;

            if (m_image != null && targetAlpha == 0f)
            {
                m_image.enabled = false;
            }

            completionSource.TrySetResult();
            m_animationRoutine = null;
        }
    }
}