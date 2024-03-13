using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Wtf
{
    [RequireComponent(typeof(Image))]
    public class ImageFlashColor : MonoBehaviour
    {
        private Coroutine m_mainRoutine;

        private ManualTimer m_timer = new ManualTimer();

        public RectTransform RectTm { get; private set; }

        public Image Image { get; private set; }

        public bool IsFlashing
        {
            get
            {
                return UnityUtils.CoroutineRunning(ref m_mainRoutine);
            }
        }

        public void StopFlash()
        {
            UnityUtils.StopCoroutine(this, ref m_mainRoutine);
            Image.color = Color.white;
        }

        public void FlashOnce(
           float duration,
           Easing.Function easing,
           Color fromColor,
           Color toColor
        )
        {
            UnityUtils.StopCoroutine(this, ref m_mainRoutine);
            m_mainRoutine = UnityUtils.StartCoroutine(this, mainRoutine(duration, easing, fromColor, toColor));
        }

        protected void Awake()
        {
            RectTm = GetComponent<RectTransform>();
            Image = GetComponent<Image>();
        }

        public async UniTask AsyncFlashOnce(
            float duration,
            Easing.Function easing,
            Color fromColor,
            Color toColor
           )
        {
            FlashOnce(duration, easing, fromColor, toColor);
            await UniTask.WaitWhile(() => IsFlashing);
        }

        private IEnumerator mainRoutine(
            float duration,
            Easing.Function entryEasing,
            Color fromColor,
            Color toColor
        )
        {
            float sourceR = fromColor.r;
            float targetR = toColor.r;
            float deltaR = targetR - sourceR;
            float sourceG = fromColor.g;
            float targetG = toColor.g;
            float deltaG = targetG - sourceG;
            float sourceB = fromColor.b;
            float targetB = toColor.b;
            float deltaB = targetB - sourceB;
            float sourceA = fromColor.a;
            float targetA = toColor.a;
            float deltaA = targetA - sourceA;

            m_timer.set(duration);

            while (!m_timer.Idle)
            {
                float v = m_timer.normalizedProgress();
                float easedV = Easing.Apply(v, entryEasing);

                Image.color = new Color(
                    sourceR + deltaR * easedV,
                    sourceG + deltaG * easedV,
                    sourceB + deltaB * easedV,
                    sourceA + deltaA * easedV);

                m_timer.tick(Time.unscaledDeltaTime);
                yield return null;
            }

            m_mainRoutine = null;
        }
    }
}
