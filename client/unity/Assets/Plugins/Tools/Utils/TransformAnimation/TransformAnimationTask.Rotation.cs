using UnityEngine;

namespace Wtf
{
    public partial class TransformAnimationTask
    {
        private struct Rotation
        {
            public bool axisX;
            public bool axisY;
            public bool axisZ;

            public Quaternion start;

            public Quaternion target;

            public Vector3 rotateBy;

            public bool local;

            public Easing.Function easing;

            public bool lazy;
        }

        public void rotate(Quaternion target, bool locally, Easing.Function easing = Easing.Function.LINEAR)
        {
            if (m_timer > 0f)
            {
                Debug.LogError("Cannot modify task after it has been started!");
                return;
            }
            m_rotation = new Rotation();
            m_has_rotation = true;
            m_rotation.axisX = true;
            m_rotation.axisY = true;
            m_rotation.axisZ = true;
            m_rotation.start = ((!locally) ? m_transform.rotation : m_transform.localRotation);
            m_rotation.target = target;
            m_rotation.local = locally;
            m_rotation.easing = easing;
            m_rotation.lazy = true;
        }

        public void rotate(Vector3 rotateBy, bool locally, Easing.Function easing = Easing.Function.LINEAR)
        {
            if (m_timer > 0f)
            {
                Debug.LogError("Cannot modify task after it has been started!");
                return;
            }
            m_rotation = new Rotation();
            m_has_rotation = true;
            m_rotation.axisX = true;
            m_rotation.axisY = true;
            m_rotation.axisZ = true;
            m_rotation.start = ((!locally) ? m_transform.rotation : m_transform.localRotation);
            m_rotation.rotateBy = rotateBy;
            m_rotation.local = locally;
            m_rotation.easing = easing;
            m_rotation.lazy = false;
        }
    }
}