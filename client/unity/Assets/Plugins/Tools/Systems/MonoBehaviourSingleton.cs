using UnityEngine;

namespace Wtf
{
    public abstract class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T sm_instance;

        public static T Inst
        {
            get
            {
                if ((Object)sm_instance == (Object)null)
                {
                    sm_instance = (T)Object.FindObjectOfType(typeof(T));
                }
                return sm_instance;
            }
        }

        abstract protected void OnAwake();

        public void Awake()
        {
            if ((Object)sm_instance == (Object)null)
            {
                sm_instance = this as T;
                DontDestroyOnLoad(base.gameObject);
                OnAwake();
            }
            else if ((Object)sm_instance != (Object)this)
            {
                Debug.LogError("Destroying duplicate singleton instance of type " + typeof(T));
                Destroy(this);
            }
        }

        public virtual void OnApplicationQuit()
        {
            sm_instance = null;
        }
    }
}