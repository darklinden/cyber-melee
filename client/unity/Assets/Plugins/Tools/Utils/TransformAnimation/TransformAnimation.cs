using System.Collections.Generic;
using UnityEngine;

namespace Wtf
{
    public class TransformAnimation : MonoBehaviour
    {
        private Dictionary<int, TransformAnimationTask> m_tasks = new Dictionary<int, TransformAnimationTask>();

        private List<int> m_queue = new List<int>();

        private int m_lastId = 0;

        public bool UseFixedUpdate { get; set; }

        private bool m_paused;

        public bool HasTasks => m_queue.Count > 0;

        public TransformAnimationTask activeOrNextAnimationTask
        {
            get
            {
                int num = peekNext();
                return (num == -1) ? null : m_tasks[num];
            }
        }

        public Transform Tm { get; private set; }

        public RectTransform RectTm { get; private set; }

        public int addTask(TransformAnimationTask animationTask)
        {
            m_lastId++;
            animationTask.Id = m_lastId;
            m_tasks.Add(m_lastId, animationTask);
            m_queue.Add(m_lastId);
            return m_lastId;
        }

        public void stopAll()
        {
            for (int i = 0; i < m_queue.Count; i++)
            {
                int key = m_queue[i];
                if (m_tasks.TryGetValue(key, out TransformAnimationTask value))
                {
                    if (value != null)
                    {
                        if (value.CancelCallback != null)
                        {
                            value.CancelCallback(key);
                        }
                        if (value.DisposeByAnimation)
                        {
                            // Log.D(gameObject.name + " stopAll DisposeByAnimation ");
                            value.Dispose();
                        }
                    }
                }
            }
            m_tasks.Clear();
            m_queue.Clear();
        }

        public void stopTask(int id)
        {
            if (m_tasks.TryGetValue(id, out var value))
            {
                if (value.CancelCallback != null)
                {
                    value.CancelCallback(id);
                }
                if (value.DisposeByAnimation)
                {
                    // Log.D(gameObject.name + " stopTask DisposeByAnimation ");
                    value.Dispose();
                }
                m_tasks.Remove(id);
            }
        }

        public void stopTask(TransformAnimationTask animationTask)
        {
            if (animationTask == null) return;
            if (animationTask.Id <= 0) return;

            stopTask(animationTask.Id);
        }

        public void pause()
        {
            m_paused = true;
        }

        public void play()
        {
            m_paused = false;
        }

        protected void Awake()
        {
            Tm = transform;
            RectTm = GetComponent<RectTransform>();
        }

        protected void FixedUpdate()
        {
            if (UseFixedUpdate)
            {
                tick();
            }
        }

        protected void Update()
        {
            if (!UseFixedUpdate)
            {
                tick();
            }
        }

        protected void OnDisable()
        {
            m_paused = false;
            stopAll();
        }

        private int peekNext()
        {
            while (m_queue.Count > 0)
            {
                int num = m_queue[0];
                if (!m_tasks.ContainsKey(num))
                {
                    // if the task is not in the dictionary, remove it from the queue
                    m_queue.RemoveAt(0);
                }
                return num;
            }
            return -1;
        }

        private void tick()
        {
            if (m_paused)
            {
                return;
            }
            int num = peekNext();
            if (num == -1)
            {
                return;
            }
            TransformAnimationTask transformAnimationTask = m_tasks[num];
            float progress = transformAnimationTask.Progress;
            transformAnimationTask.apply();
            if (progress == 0f && transformAnimationTask.Progress > 0f && transformAnimationTask.StartCallback != null)
            {
                transformAnimationTask.StartCallback(this);
            }
            if (transformAnimationTask.Completed)
            {
                m_tasks.Remove(num);
                m_queue.RemoveAt(0);
                if (transformAnimationTask.EndCallback != null)
                {
                    transformAnimationTask.EndCallback(this);
                }

                if (transformAnimationTask.DisposeByAnimation)
                {
                    // Log.D(gameObject.name + " tick DisposeByAnimation ");
                    transformAnimationTask.Dispose();
                }
            }
        }
    }

}