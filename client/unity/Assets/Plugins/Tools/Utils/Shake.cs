using System.Collections;
using UnityEngine;

namespace Wtf
{
    public class Shake : MonoBehaviour
    {
        [SerializeField, ReadOnly] private Vector3 m_startPos;

        [SerializeField, ReadOnly] private float m_decay;

        [SerializeField, ReadOnly] private float m_intensity;

        protected Transform m_Tm;
        public Transform Tm
        {
            get
            {
                if (m_Tm == null)
                {
                    m_Tm = transform;
                }
                return m_Tm;
            }
        }

        private Coroutine m_shakeRoutine = null;

        public bool Shaking
        {
            get
            {
                return m_shakeRoutine != null || m_intensity > 0f;
            }
        }

        public void EndShake()
        {
            if (m_intensity > 0f)
            {
                UnityUtils.StopCoroutine(this, ref m_shakeRoutine);

                m_intensity = 0f;
                Tm.position = m_startPos;
            }
        }

        public void PlayShake(float intensity, float decay)
        {
            if (!Shaking)
            {
                m_intensity = intensity;
                m_decay = decay * 60f;
                m_startPos = Tm.position;

                UnityUtils.StartCoroutine(this, ShakeRoutine());
            }
        }

        protected IEnumerator ShakeRoutine()
        {
            while (m_intensity > 0f)
            {
                Tm.position = m_startPos + Random.insideUnitSphere * m_intensity;
                m_intensity -= m_decay * Time.unscaledDeltaTime;
                if (m_intensity <= 0f)
                {
                    Tm.position = m_startPos;
                }

                yield return null;
            }
        }

        [ContextMenu("TestShake")]
        private void TestShake()
        {
            PlayShake(10f, 10f);
        }
    }
}