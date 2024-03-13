using System;
using UnityEngine;

namespace Wtf
{
    public partial class TransformAnimationTask : IDisposable
    {
        public int Id { get; set; }
        public bool DisposeByAnimation { get; set; } = true;

        public static TransformAnimationTask Get(
            Transform tm,
            float duration,
            float delay = 0f,
            TimeUtil.DeltaTimeType dtType = TimeUtil.DeltaTimeType.UNSCALED_DELTA_TIME)
        {
            var task = AnyPool<TransformAnimationTask>.Get();
            task.Initialize(tm, duration, delay, dtType);
            task.DisposeByAnimation = true;
            return task;
        }

        public static TransformAnimationTask Get(
            Transform tm,
            TransformAnimationTask preset,
            TimeUtil.DeltaTimeType dtType = TimeUtil.DeltaTimeType.UNSCALED_DELTA_TIME)
        {
            var task = AnyPool<TransformAnimationTask>.Get();
            task.Initialize(tm, preset, dtType);
            task.DisposeByAnimation = true;
            return task;
        }

        public void Dispose()
        {
            Id = -1;
            m_transform = null;
            m_rectTransform = null;
            m_startCallback = null;
            m_endCallback = null;
            m_cancelCallback = null;
            m_has_translation = false;
            m_has_translationAnchoredPosition = false;
            m_has_rotation = false;
            m_has_scaling = false;
            m_animation_action = null;
            m_timer = 0f;
            m_duration = 0f;
            m_delay = 0f;
            m_dtType = TimeUtil.DeltaTimeType.UNSCALED_DELTA_TIME;

            AnyPool<TransformAnimationTask>.Return(this);
        }

        private Transform m_transform;

        private RectTransform m_rectTransform;

        private float m_timer;

        private float m_duration;

        private float m_delay;

        private TimeUtil.DeltaTimeType m_dtType;

        private Action<TransformAnimation> m_startCallback;

        private Action<TransformAnimation> m_endCallback;

        private Action<int> m_cancelCallback;

        private bool m_has_translation = false;
        private Translation m_translation;

        private bool m_has_translationAnchoredPosition = false;
        private TranslationAnchoredPosition m_translationAnchoredPosition;

        private bool m_has_rotation = false;
        private Rotation m_rotation;

        private bool m_has_scaling = false;
        private Scaling m_scaling;

        public bool Completed
        {
            get
            {
                return m_timer == m_duration;
            }
        }

        public float Progress
        {
            get
            {
                return m_timer / m_duration;
            }
        }

        public Action<TransformAnimation> StartCallback
        {
            get
            {
                return m_startCallback;
            }
            set
            {
                m_startCallback = value;
            }
        }

        public Action<TransformAnimation> EndCallback
        {
            get
            {
                return m_endCallback;
            }
            set
            {
                m_endCallback = value;
            }
        }

        public Action<int> CancelCallback
        {
            get
            {
                return m_cancelCallback;
            }
            set
            {
                m_cancelCallback = value;
            }
        }

        public float Duration
        {
            get
            {
                return m_duration;
            }
        }

        public Vector3 TargetPosition
        {
            get
            {
                return m_translation.target;
            }
        }

        public Vector2 TargetAnchoredPosition
        {
            get
            {
                return m_translationAnchoredPosition.target;
            }
        }

        public Quaternion TargetRotation
        {
            get
            {
                return m_rotation.target;
            }
        }

        public Vector3 TargetScale
        {
            get
            {
                return m_scaling.target;
            }
        }

        public void Initialize(
            Transform tm,
            float duration,
            float delay = 0f,
            TimeUtil.DeltaTimeType dtType = TimeUtil.DeltaTimeType.UNSCALED_DELTA_TIME)
        {
            m_transform = tm;
            m_rectTransform = ((!(tm is RectTransform)) ? null : ((RectTransform)tm));
            m_timer = 0f;
            m_duration = duration;
            m_delay = delay;
            m_dtType = dtType;
        }

        public void Initialize(
            Transform tm,
            TransformAnimationTask preset,
            TimeUtil.DeltaTimeType dtType = TimeUtil.DeltaTimeType.UNSCALED_DELTA_TIME)
        {
            m_transform = tm;
            m_rectTransform = ((!(tm is RectTransform)) ? null : ((RectTransform)tm));
            if (preset.m_has_translation)
            {
                var translation = new Translation();
                translation.axisX = preset.m_translation.axisX;
                translation.axisY = preset.m_translation.axisY;
                translation.axisZ = preset.m_translation.axisZ;
                translation.start = ((!preset.m_translation.local) ? m_transform.position : m_transform.localPosition);
                translation.target = preset.m_translation.target;
                translation.local = preset.m_translation.local;
                translation.easing = preset.m_translation.easing;
                m_translation = translation;
            }
            if (preset.m_has_translationAnchoredPosition)
            {
                var translationAnchoredPosition = new TranslationAnchoredPosition();
                translationAnchoredPosition.start = m_rectTransform.anchoredPosition;
                translationAnchoredPosition.target = preset.m_translationAnchoredPosition.target;
                translationAnchoredPosition.easing = preset.m_translationAnchoredPosition.easing;
                m_translationAnchoredPosition = translationAnchoredPosition;
            }
            if (preset.m_has_rotation)
            {
                var rotation = new Rotation();
                rotation.axisX = preset.m_rotation.axisX;
                rotation.axisY = preset.m_rotation.axisY;
                rotation.axisZ = preset.m_rotation.axisZ;
                rotation.start = ((!preset.m_rotation.local) ? m_transform.rotation : m_transform.localRotation);
                rotation.target = preset.m_rotation.target;
                rotation.rotateBy = preset.m_rotation.rotateBy;
                rotation.local = preset.m_rotation.local;
                rotation.easing = preset.m_rotation.easing;
                rotation.lazy = preset.m_rotation.lazy;
                m_rotation = rotation;
            }
            if (preset.m_has_scaling)
            {
                var scaling = new Scaling();
                scaling.axisX = preset.m_scaling.axisX;
                scaling.axisY = preset.m_scaling.axisY;
                scaling.axisZ = preset.m_scaling.axisZ;
                scaling.start = ((!preset.m_scaling.local) ? m_transform.lossyScale : m_transform.localScale);
                scaling.target = preset.m_scaling.target;
                scaling.local = preset.m_scaling.local;
                scaling.easing = preset.m_scaling.easing;
                m_scaling = scaling;
            }
            m_timer = 0f;
            m_duration = preset.m_duration;
            m_delay = preset.m_delay;
            m_dtType = dtType;
        }

        public void reset()
        {
            m_timer = 0f;
        }

        public void reset(float duration, float delay, Easing.Function easingFunction)
        {
            reset();
            m_duration = duration;
            m_delay = delay;
            if (m_has_translation)
            {
                m_translation.easing = easingFunction;
            }
            if (m_has_translationAnchoredPosition)
            {
                m_translationAnchoredPosition.easing = easingFunction;
            }
            if (m_has_rotation)
            {
                m_rotation.easing = easingFunction;
            }
            if (m_has_scaling)
            {
                m_scaling.easing = easingFunction;
            }
        }

        public void setTranslationAxises(bool x, bool y, bool z)
        {
            if (!m_has_translation)
            {
                throw new Exception("Translation is not set");
            }
            m_translation.axisX = x;
            m_translation.axisY = y;
            m_translation.axisZ = z;
        }

        public void setRotationAxises(bool x, bool y, bool z)
        {
            if (!m_has_rotation)
            {
                throw new Exception("Rotation is not set");
            }
            m_rotation.axisX = x;
            m_rotation.axisY = y;
            m_rotation.axisZ = z;
        }

        public void setScalingAxises(bool x, bool y, bool z)
        {
            if (!m_has_scaling)
            {
                throw new Exception("Scaling is not set");
            }
            m_scaling.axisX = x;
            m_scaling.axisY = y;
            m_scaling.axisZ = z;
        }

        public void apply()
        {
            float deltaTime = TimeUtil.GetDeltaTime(m_dtType);
            float num = 0f;
            if (m_delay > 0f)
            {
                m_delay -= deltaTime;
                if (m_delay > 0f)
                {
                    return;
                }
                num = m_delay * -1f;
                m_delay = 0f;
            }
            if (m_timer == 0f)
            {
                if (m_has_translation)
                {
                    m_translation.start = ((!m_translation.local) ? m_transform.position : m_transform.localPosition);
                }
                if (m_has_translationAnchoredPosition)
                {
                    m_translationAnchoredPosition.start = m_rectTransform.anchoredPosition;
                }
                if (m_has_rotation)
                {
                    m_rotation.start = ((!m_rotation.local) ? m_transform.rotation : m_transform.localRotation);
                }
                if (m_has_scaling)
                {
                    m_scaling.start = ((!m_scaling.local) ? m_transform.lossyScale : m_transform.localScale);
                }
                if (m_animation_action != null)
                {
                    m_animation_action.Invoke(0);
                }
            }
            m_timer = Mathf.Min(m_timer + deltaTime + num, m_duration);
            float progress = ((!(m_duration > 0f)) ? 1f : Mathf.Clamp01(m_timer / m_duration));
            if (m_has_translation)
            {
                applyTranslation(progress);
            }
            if (m_has_translationAnchoredPosition)
            {
                applyTranslationToAnchoredPos(progress);
            }
            if (m_has_rotation)
            {
                applyRotation(progress);
            }
            if (m_has_scaling)
            {
                applyScaling(progress);
            }
            applyAnimationAction(progress);
        }

        private void applyTranslation(float progress)
        {
            Vector3 vector = Vector3.zero;
            float num = Easing.Apply(progress, m_translation.easing);
            if (m_translation.algorithm == TranslationAlgorithm.LINEAR)
            {
                vector = m_translation.start + (m_translation.target - m_translation.start) * num;
            }
            else if (m_translation.algorithm == TranslationAlgorithm.CATMULL_ROM)
            {
                vector = MathUtil.CatmullRom(num, m_translation.start + m_translation.cp0, m_translation.start, m_translation.target, m_translation.target + m_translation.cp1);
            }
            else if (m_translation.algorithm == TranslationAlgorithm.QUADRATIC_BEZIER)
            {
                vector = MathUtil.QuadraticBezier(num, m_translation.start, m_translation.cp0, m_translation.target);
            }
            else if (m_translation.algorithm == TranslationAlgorithm.CUBIC_BEZIER)
            {
                vector = MathUtil.CubicBezier(num, m_translation.start, m_translation.cp0, m_translation.cp1, m_translation.target);
            }
            else if (m_translation.algorithm == TranslationAlgorithm.QUARTIC_BEZIER)
            {
                vector = MathUtil.QuarticBezier(num, m_translation.start, m_translation.cp0, m_translation.cp1, m_translation.cp2, m_translation.target);
            }
            if (m_translation.local)
            {
                m_transform.localPosition = new Vector3(
                    !m_translation.axisX ? m_transform.localPosition.x : vector.x,
                    !m_translation.axisY ? m_transform.localPosition.y : vector.y,
                    !m_translation.axisZ ? m_transform.localPosition.z : vector.z
                );
            }
            else
            {
                m_transform.position = new Vector3(
                    !m_translation.axisX ? m_transform.position.x : vector.x,
                    !m_translation.axisY ? m_transform.position.y : vector.y,
                    !m_translation.axisZ ? m_transform.position.z : vector.z
                );
            }
        }

        private void applyTranslationToAnchoredPos(float progress)
        {
            Vector2 vector = Vector2.zero;
            float num = Easing.Apply(progress, m_translationAnchoredPosition.easing);
            if (m_translationAnchoredPosition.algorithm == TranslationAlgorithm.LINEAR)
            {
                vector = m_translationAnchoredPosition.start + (m_translationAnchoredPosition.target - m_translationAnchoredPosition.start) * num;
            }
            m_rectTransform.anchoredPosition = new Vector2(vector.x, vector.y);
        }

        private void applyRotation(float progress)
        {
            float num = Easing.Apply(progress, m_rotation.easing);
            Vector3 eulerAngles = ((!m_rotation.lazy) ? Quaternion.Euler(m_rotation.start.eulerAngles + m_rotation.rotateBy * num) : Quaternion.Lerp(m_rotation.start, m_rotation.target, num)).eulerAngles;
            if (m_rotation.local)
            {
                Vector3 eulerAngles2 = m_transform.localRotation.eulerAngles;
                m_transform.localRotation = Quaternion.Euler(new Vector3(
                    !m_rotation.axisX ? eulerAngles2.x : eulerAngles.x,
                    !m_rotation.axisY ? eulerAngles2.y : eulerAngles.y,
                    !m_rotation.axisZ ? eulerAngles2.z : eulerAngles.z
                ));
            }
            else
            {
                Vector3 eulerAngles3 = m_transform.rotation.eulerAngles;
                m_transform.rotation = Quaternion.Euler(new Vector3(
                    !m_rotation.axisX ? eulerAngles3.x : eulerAngles.x,
                    !m_rotation.axisY ? eulerAngles3.y : eulerAngles.y,
                    !m_rotation.axisZ ? eulerAngles3.z : eulerAngles.z
                ));
            }
        }

        private void applyScaling(float progress)
        {
            float num = Easing.Apply(progress, m_scaling.easing);
            Vector3 vector = m_scaling.start + (m_scaling.target - m_scaling.start) * num;
            if (m_scaling.local)
            {
                m_transform.localScale = new Vector3(
                    !m_scaling.axisX ? m_transform.localScale.x : vector.x,
                    !m_scaling.axisY ? m_transform.localScale.y : vector.y,
                    !m_scaling.axisZ ? m_transform.localScale.z : vector.z
                );
                return;
            }
            else
            {
                vector = new Vector3(
                    !m_scaling.axisX ? m_transform.lossyScale.x : vector.x,
                    !m_scaling.axisY ? m_transform.lossyScale.y : vector.y,
                    !m_scaling.axisZ ? m_transform.lossyScale.z : vector.z
                );
                m_transform.SetWorldScale(vector);
            }
        }

        private void applyAnimationAction(float progress)
        {
            if (m_animation_action != null)
            {
                float num = Easing.Apply(progress, m_animation_action_easing);
                m_animation_action.Invoke(num);
            }
        }
    }
}