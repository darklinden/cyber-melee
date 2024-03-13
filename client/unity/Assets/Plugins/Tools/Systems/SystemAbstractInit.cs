using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Wtf
{
    public abstract class SystemAbstractInit : MonoBehaviour, ISystemBase
    {
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

        public bool IsInitialized { get; private set; }
        public Dictionary<string, ISystemBase> SubSystems { get; } = null;

        public virtual void Initialize()
        {
            IsInitialized = true;
        }

        public virtual UniTask AsyncInitialize()
        {
            Log.E(this.GetType().Name, "AsyncInitialize");
            throw new System.NotImplementedException();
        }

        public virtual void Deinitialize()
        {
            IsInitialized = false;
        }
    }
}