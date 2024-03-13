using System.Collections;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wtf;

namespace App
{
    public class Splash : MonoBehaviour
    {
        const float FADE_TO_BLACK_DURATION = 0.2f;

        public CanvasGroupAlphaFading CanvasGroup;

        public GameObject ImageRoot;

        public TMPro.TextMeshProUGUI BuildInfoVersion;

        public AnimatedProgressBar ProgressBar;

        public Transform Tm { get; private set; }

        public bool IsVisible
        {
            get
            {
                return gameObject.activeSelf;
            }
        }

        public Canvas Canvas { get; set; }

        protected void Awake()
        {
            Tm = transform;
            Canvas = GetComponent<Canvas>();
            SetProgressBar(0f);
            if (BuildInfoVersion != null)
                BuildInfoVersion.text = App.Constants.VERSION;
        }

        public void SetProgressBar(float interval)
        {
            if (ProgressBar != null)
                ProgressBar.SetNormalizedValue(interval);
        }

        public void UpdateBuildInfoVersion(string version)
        {
            if (BuildInfoVersion != null)
                BuildInfoVersion.text = version;
        }

        public void AnimToProgress(float interval, float duration = 0.4f)
        {
            AsyncAnimToProgress(interval, duration).Forget();
        }

        public async UniTask AsyncAnimToProgress(float interval, float duration = 0.4f)
        {
            if (ProgressBar != null)
                await ProgressBar.AsyncAnimateToValue(interval, duration);
        }

        Coroutine m_animationRoutine = null;

        public void SetVisible(bool visible)
        {
            Log.D("Splash.setVisible: ", visible);
            if (visible)
            {
                if (ProgressBar != null)
                    ProgressBar.SetNormalizedValue(0f);
            }

            UnityUtils.StopCoroutine(this, ref m_animationRoutine);

            CanvasGroup.setTransparent(!visible);
            gameObject.SetActive(visible);
        }

        public async UniTask AsyncAnimateToBlack()
        {
            gameObject.SetActive(true);
            UnityUtils.StopCoroutine(this, ref m_animationRoutine);

            var completionSource = AutoResetUniTaskCompletionSource.Create();
            m_animationRoutine = StartCoroutine(AnimationRoutine(true, FADE_TO_BLACK_DURATION, completionSource));
            try
            {
                await completionSource.Task;
            }
            catch (System.OperationCanceledException)
            {
                // ignore
            }
        }

        public async UniTask AsyncAnimateToTransparent()
        {
            UnityUtils.StopCoroutine(this, ref m_animationRoutine);
            if (!gameObject.activeSelf)
            {
                SetVisible(false);
                return;
            }

            var completionSource = AutoResetUniTaskCompletionSource.Create();
            m_animationRoutine = UnityUtils.StartCoroutine(this, AnimationRoutine(false, FADE_TO_BLACK_DURATION * 2f, completionSource));

            try
            {
                await completionSource.Task;
            }
            catch (System.OperationCanceledException)
            {
                // ignore
            }
        }

        private IEnumerator AnimationRoutine(bool toBlack, float duration, AutoResetUniTaskCompletionSource completionSource)
        {
            if (toBlack)
            {
                yield return CanvasGroup.animateToBlack(duration);
            }
            else
            {
                yield return CanvasGroup.animateToTransparent(duration).ToCoroutine();
            }
            m_animationRoutine = null;

            if (!toBlack)
            {
                gameObject.SetActive(false);
            }

            completionSource.TrySetResult();
        }

    }
}
