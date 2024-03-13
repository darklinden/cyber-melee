using System.Reflection;
using UnityEngine;

namespace Wtf
{
    public class ButtonCallSystem : AbstractUIButton
    {
        [SerializeField] private string SystemRootName;
        [SerializeField] private string[] SystemNames;
        [SerializeField] private string MethodName;

        private ISystemBase m_SystemRoot = null;
        private ISystemBase SystemRoot
        {
            get
            {
                if (string.IsNullOrEmpty(SystemRootName))
                {
                    return null;
                }

                // 使用反射获取系统
                if (m_SystemRoot == null)
                {
                    var type = System.Type.GetType(SystemRootName);
                    if (type != null)
                    {
                        m_SystemRoot = type.GetProperty("Inst", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).GetValue(null) as ISystemBase;
                    }
                    else
                    {
                        Log.E("ButtonCallSystem.SystemRoot type is null", SystemRootName);
                    }

                    if (m_SystemRoot == null)
                    {
                        Log.E("ButtonCallSystem.SystemRoot is null");
                    }
                }

                return m_SystemRoot;
            }
        }

        public override void OnButtonClicked(GameObject go)
        {
            var root = SystemRoot;
            if (root == null)
            {
                Log.E("ButtonCallSystem.OnButtonClicked root is null");
                return;
            }

            ISystemBase currentSystem = root;
            for (int i = 0; i < SystemNames.Length; i++)
            {
                var sys = currentSystem.GetSystem(SystemNames[i]);

                if (sys == null)
                {
                    Log.E("ButtonCallSystem.OnButtonClicked system is null");
                    return;
                }

                currentSystem = sys;
            }

            var method = currentSystem.GetType().GetMethod(MethodName);
            if (method == null)
            {
                Log.E("ButtonCallSystem.OnButtonClicked method is null");
                return;
            }

            method.Invoke(currentSystem, null);
        }
    }
}