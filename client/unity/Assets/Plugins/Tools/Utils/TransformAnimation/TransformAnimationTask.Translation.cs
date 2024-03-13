using UnityEngine;

namespace Wtf
{
    public partial class TransformAnimationTask
    {
        private struct Translation
        {
            public bool axisX;
            public bool axisY;
            public bool axisZ;

            public Vector3 start;

            public Vector3 target;

            public bool local;

            public Easing.Function easing;

            public TranslationAlgorithm algorithm;

            public Vector3 cp0;

            public Vector3 cp1;

            public Vector3 cp2;
        }

        private struct TranslationAnchoredPosition
        {
            public Vector2 start;

            public Vector2 target;

            public Easing.Function easing;

            public TranslationAlgorithm algorithm;
        }

        public void translate(Vector3 target, bool locally, Easing.Function easing = Easing.Function.LINEAR)
        {
            if (m_timer > 0f)
            {
                Debug.LogError("Cannot modify task after it has been started!");
                return;
            }
            m_translation = new Translation();
            m_has_translation = true;
            m_translation.axisX = true;
            m_translation.axisY = true;
            m_translation.axisZ = true;
            m_translation.start = ((!locally) ? m_transform.position : m_transform.localPosition);
            m_translation.target = target;
            m_translation.local = locally;
            m_translation.easing = easing;
            m_translation.algorithm = TranslationAlgorithm.LINEAR;
        }

        public void translateToAnchoredPos(Vector2 targetAnchoredPos, Easing.Function easing = Easing.Function.LINEAR)
        {
            if (m_timer > 0f)
            {
                Debug.LogError("Cannot modify task after it has been started!");
                return;
            }
            if (m_rectTransform == null)
            {
                Debug.LogError("No RectTransform specified before attempting to translate to anchored position!");
                return;
            }
            m_translationAnchoredPosition = new TranslationAnchoredPosition();
            m_has_translationAnchoredPosition = true;
            m_translationAnchoredPosition.start = m_rectTransform.anchoredPosition;
            m_translationAnchoredPosition.target = targetAnchoredPos;
            m_translationAnchoredPosition.easing = easing;
            m_translationAnchoredPosition.algorithm = TranslationAlgorithm.LINEAR;
        }

        public void translateWithCatmullRom(Vector3 target, bool locally, Vector3 catmullRomP0, Vector3 catmullRomP1, Easing.Function easing = Easing.Function.LINEAR)
        {
            if (m_timer > 0f)
            {
                Debug.LogError("Cannot modify task after it has been started!");
                return;
            }
            m_translation = new Translation();
            m_has_translation = true;
            m_translation.axisX = true;
            m_translation.axisY = true;
            m_translation.axisZ = true;
            m_translation.start = ((!locally) ? m_transform.position : m_transform.localPosition);
            m_translation.target = target;
            m_translation.local = locally;
            m_translation.easing = easing;
            m_translation.algorithm = TranslationAlgorithm.CATMULL_ROM;
            m_translation.cp0 = catmullRomP0;
            m_translation.cp1 = catmullRomP1;
        }

        public void translateWithQuadraticBezier(Vector3 target, bool locally, Vector3 bezierP1, Easing.Function easing = Easing.Function.LINEAR)
        {
            if (m_timer > 0f)
            {
                Debug.LogError("Cannot modify task after it has been started!");
                return;
            }
            m_translation = new Translation();
            m_has_translation = true;
            m_translation.axisX = true;
            m_translation.axisY = true;
            m_translation.axisZ = true;
            m_translation.start = ((!locally) ? m_transform.position : m_transform.localPosition);
            m_translation.target = target;
            m_translation.local = locally;
            m_translation.easing = easing;
            m_translation.algorithm = TranslationAlgorithm.QUADRATIC_BEZIER;
            m_translation.cp0 = bezierP1;
        }

        public void translateWithCubicBezier(Vector3 target, bool locally, Vector3 bezierP1, Vector3 bezierP2, Easing.Function easing = Easing.Function.LINEAR)
        {
            if (m_timer > 0f)
            {
                Debug.LogError("Cannot modify task after it has been started!");
                return;
            }
            m_translation = new Translation();
            m_has_translation = true;
            m_translation.axisX = true;
            m_translation.axisY = true;
            m_translation.axisZ = true;
            m_translation.start = ((!locally) ? m_transform.position : m_transform.localPosition);
            m_translation.target = target;
            m_translation.local = locally;
            m_translation.easing = easing;
            m_translation.algorithm = TranslationAlgorithm.CUBIC_BEZIER;
            m_translation.cp0 = bezierP1;
            m_translation.cp1 = bezierP2;
        }

        public void translateWithQuarticBezier(Vector3 target, bool locally, Vector3 bezierP1, Vector3 bezierP2, Vector3 bezierP3, Easing.Function easing = Easing.Function.LINEAR)
        {
            if (m_timer > 0f)
            {
                Debug.LogError("Cannot modify task after it has been started!");
                return;
            }
            m_translation = new Translation();
            m_translation.axisX = true;
            m_translation.axisY = true;
            m_translation.axisZ = true;
            m_translation.start = ((!locally) ? m_transform.position : m_transform.localPosition);
            m_translation.target = target;
            m_translation.local = locally;
            m_translation.easing = easing;
            m_translation.algorithm = TranslationAlgorithm.QUARTIC_BEZIER;
            m_translation.cp0 = bezierP1;
            m_translation.cp1 = bezierP2;
            m_translation.cp2 = bezierP3;
        }
    }
}