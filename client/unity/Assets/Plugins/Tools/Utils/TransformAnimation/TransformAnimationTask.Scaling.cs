using UnityEngine;

namespace Wtf
{
    public partial class TransformAnimationTask
    {
        private struct Scaling
        {
            public bool axisX;
            public bool axisY;
            public bool axisZ;

            public Vector3 start;

            public Vector3 target;

            public bool local;

            public Easing.Function easing;
        }

        public void scale(Vector3 target, bool locally, Easing.Function easing = Easing.Function.LINEAR)
        {
            if (m_timer > 0f)
            {
                Debug.LogError("Cannot modify task after it has been started!");
                return;
            }
            m_scaling = new Scaling();
            m_has_scaling = true;
            m_scaling.axisX = true;
            m_scaling.axisY = true;
            m_scaling.axisZ = true;
            m_scaling.start = ((!locally) ? m_transform.lossyScale : m_transform.localScale);
            m_scaling.target = target;
            m_scaling.local = locally;
            m_scaling.easing = easing;
        }
    }
}