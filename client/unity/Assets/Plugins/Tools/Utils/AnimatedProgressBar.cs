using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Wtf
{
    public class AnimatedProgressBar : MonoBehaviour
    {
        /// <summary>
        /// Setting that indicates one of four directions.
        /// </summary>
        public enum Direction
        {
            /// <summary>
            /// From the left to the right
            /// </summary>
            LeftToRight,

            /// <summary>
            /// From the right to the left
            /// </summary>
            RightToLeft,

            /// <summary>
            /// From the bottom to the top.
            /// </summary>
            BottomToTop,

            /// <summary>
            /// From the top to the bottom.
            /// </summary>
            TopToBottom,
        }

        [SerializeField]
        private Direction m_Direction = Direction.LeftToRight;

        enum Axis
        {
            Horizontal = 0,
            Vertical = 1
        }

        Axis axis { get { return (m_Direction == Direction.LeftToRight || m_Direction == Direction.RightToLeft) ? Axis.Horizontal : Axis.Vertical; } }
        bool reverseValue { get { return m_Direction == Direction.RightToLeft || m_Direction == Direction.TopToBottom; } }


#if UNITY_EDITOR && DEBUG
        [SerializeField]
#endif
        private float m_Value = -1;

        public float Value
        {
            get
            {
                if (m_Value < 0)
                {
                    m_Value = Slider.fillAmount;
                }
                return m_Value;
            }
            set
            {
                if (m_Value != value)
                {
                    m_Value = Mathf.Clamp01(value);
                    Slider.fillAmount = m_Value;

                    if (Handler != null)
                    {
                        Vector2 anchorMin = Vector2.zero;
                        Vector2 anchorMax = Vector2.one;
                        anchorMin[(int)axis] = anchorMax[(int)axis] = (reverseValue ? (1 - m_Value) : m_Value);
                        Handler.anchorMin = anchorMin;
                        Handler.anchorMax = anchorMax;
                    }
                }
            }
        }

        public Easing.Function DefaultEasing { get; set; } = Easing.Function.LINEAR;

        private ManualTimer m_timer = new ManualTimer();

        private Coroutine m_animationRoutine;

        public bool Animating
        {
            get
            {
                return UnityUtils.CoroutineRunning(ref m_animationRoutine);
            }
        }

        public Image BackGround;
        public Image Slider;
        public RectTransform Handler;

        protected void Awake()
        {
            if (BackGround == null)
            {
                BackGround = GetComponent<Image>();
            }
            BackGround.raycastTarget = false;

            if (Slider == null)
            {
                var contents = BackGround.GetComponentsInChildren<Image>(true);
                if (contents.Length > 1)
                {
                    Slider = contents[1];
                }
            }
            Slider.raycastTarget = false;
            Slider.type = Image.Type.Filled;
            Slider.fillMethod = axis == Axis.Horizontal ? Image.FillMethod.Horizontal : Image.FillMethod.Vertical;
            Slider.fillOrigin = axis == Axis.Horizontal ?
                reverseValue ? (int)Image.OriginHorizontal.Right : (int)Image.OriginHorizontal.Left :
                reverseValue ? (int)Image.OriginVertical.Top : (int)Image.OriginVertical.Bottom;
        }

        public void SetNormalizedValue(float v)
        {
            UnityUtils.StopCoroutine(this, ref m_animationRoutine);
            Value = v;
        }

        public void StopAnimate()
        {
            UnityUtils.StopCoroutine(this, ref m_animationRoutine);
        }

        public void AnimateToValue(
            float sourceV,
            float targetV,
            float duration,
            Easing.Function? easingFunction = null,
            float delay = 0f)
        {
            AsyncAnimateToValue(sourceV, targetV, duration, easingFunction, delay).Forget();
        }

        public void AnimateToValue(
            float targetV,
            float duration,
            Easing.Function? easingFunction = null,
            float delay = 0f)
        {
            AsyncAnimateToValue(Value, targetV, duration, easingFunction, delay).Forget();
        }

        public async UniTask AsyncAnimateToValue(
            float targetV,
            float duration,
            Easing.Function? easingFunction = null,
            float delay = 0f)
        {
            await AsyncAnimateToValue(Value, targetV, duration, easingFunction, delay);
        }

        public async UniTask AsyncAnimateToValue(
            float sourceV,
            float targetV,
            float duration,
            Easing.Function? easingFunction = null,
            float delay = 0f)
        {
            if (!gameObject.activeInHierarchy)
            {
                SetNormalizedValue(targetV);
                return;
            }

            if (!easingFunction.HasValue)
            {
                easingFunction = DefaultEasing;
            }

            UnityUtils.StopCoroutine(this, ref m_animationRoutine);

            var animationCompletionSource = AutoResetUniTaskCompletionSource.Create();

            m_animationRoutine = UnityUtils.StartCoroutine(this,
                animatedProgressRoutine(sourceV, targetV, duration, easingFunction.Value, delay, animationCompletionSource));

            try
            {
                await animationCompletionSource.Task;
            }
            catch (OperationCanceledException)
            {
                // do nothing
            }
        }

        private IEnumerator animatedProgressRoutine(
            float sourceV,
            float targetV,
            float duration,
            Easing.Function easingFunction,
            float delay,
            AutoResetUniTaskCompletionSource completeSource)
        {
            if (delay > 0f)
            {
                yield return Yielders.WaitForSecondsRealtime(delay);
            }

            // 如果两个值相等，直接返回
            if (Mathf.Abs(Value - targetV) < 0.001f)
            {
                Value = targetV;
                yield break;
            }

            // 如果两个值相差很小，直接返回
            if (Mathf.Abs(sourceV - targetV) < 0.001f)
            {
                Value = targetV;
                yield break;
            }

            Value = sourceV;
            float deltaV = targetV - sourceV;
            m_timer.set(duration);
            while (!m_timer.Idle)
            {
                float eased = Easing.Apply(m_timer.normalizedProgress(), easingFunction);
                float currentV = sourceV + deltaV * eased;
                Value = currentV;
                m_timer.tick(Time.unscaledDeltaTime);
                yield return null;
            }

            Value = targetV;

            if (completeSource != null)
            {
                completeSource.TrySetResult();
            }
            m_animationRoutine = null;
        }
    }
}