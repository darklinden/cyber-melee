using System;
using System.Collections;
using UnityEngine;

namespace Wtf
{
    public class ManualTimer : IDisposable
    {
        private string m_debugInfo = string.Empty;
        private float m_originalTimeSet;
        private float m_timeRemaining;
        private bool m_idle;
        public bool Idle
        {
            get
            {
                if (!string.IsNullOrEmpty(m_debugInfo))
                {
                    Log.D("ManualTimer.Idle get: ", m_idle);
                }
                return m_idle;
            }
            private set
            {
                if (!string.IsNullOrEmpty(m_debugInfo))
                {
                    Log.D("ManualTimer.Idle set: ", value);
                }
                m_idle = value;
            }
        }

        public ManualTimer()
        {
            m_timeRemaining = 0f;
            Idle = true;
        }

        public ManualTimer(string debugInfo)
        {
            m_debugInfo = debugInfo;
            m_timeRemaining = 0f;
            Idle = true;
        }

        public ManualTimer(float duration, string debugInfo = null)
        {
            m_debugInfo = debugInfo;
            set(duration);
        }

        public void set(float duration)
        {
            m_originalTimeSet = duration;
            m_timeRemaining = duration;
            Idle = false;
        }

        public void addAndClamp(float addedSeconds)
        {
            m_timeRemaining = Mathf.Clamp(m_timeRemaining + addedSeconds, 0f, m_originalTimeSet);
            if (m_timeRemaining > 0f)
            {
                Idle = false;
            }
        }

        public void reset()
        {
            m_timeRemaining = m_originalTimeSet;
            Idle = false;
        }

        public void end()
        {
            m_timeRemaining = 0f;
            Idle = true;
        }

        public bool tick(float dt)
        {
            if (!string.IsNullOrEmpty(m_debugInfo))
            {
                Log.D("ManualTimer.tick ", m_debugInfo, "Idle;", Idle, "dt:", dt, "timeRemaining:", m_timeRemaining);
            }

            if (Idle)
            {
                return false;
            }

            m_timeRemaining -= dt;
            if (m_timeRemaining <= 0f)
            {
                end();
                return true;
            }
            return false;
        }

        public float timeRemaining()
        {
            return m_timeRemaining;
        }

        public float timeElapsed()
        {
            return m_originalTimeSet - m_timeRemaining;
        }

        public float originalTimeSet()
        {
            return m_originalTimeSet;
        }

        public float normalizedProgress()
        {
            if (m_originalTimeSet <= 0f)
            {
                return 1f;
            }
            return Mathf.Clamp01(timeElapsed() / m_originalTimeSet);
        }

        public IEnumerator tickUntilEndUnscaled()
        {
            while (!Idle && !tick(Time.unscaledDeltaTime))
            {
                yield return null;
            }
        }

        public static ManualTimer Get(string debugInfo = null)
        {
            var timer = AnyPool<ManualTimer>.Get();
            timer.m_debugInfo = debugInfo;
            return timer;
        }

        public static ManualTimer Get(float duration, string debugInfo = null)
        {
            ManualTimer manualTimer = AnyPool<ManualTimer>.Get();
            manualTimer.m_debugInfo = debugInfo;
            manualTimer.set(duration);
            return manualTimer;
        }

        public void Dispose()
        {
            AnyPool<ManualTimer>.Return(this);
        }
    }
}