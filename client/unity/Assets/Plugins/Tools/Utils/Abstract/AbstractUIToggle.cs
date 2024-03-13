using UnityEngine;
using UnityEngine.UI;

namespace Wtf
{
    public abstract class AbstractUIToggle : MonoBehaviour
    {
        public Toggle toggle;

#if UNITY_EDITOR
        [ContextMenu("Setup")]
        protected void Setup()
        {
            if (toggle == null)
            {
                toggle = this.GetComponent<Toggle>();
            }

            var persistentEventCount = toggle.onValueChanged.GetPersistentEventCount();
            if (persistentEventCount > 0)
            {
                for (int i = persistentEventCount - 1; i >= 0; i--)
                    UnityEditor.Events.UnityEventTools.RemovePersistentListener(toggle.onValueChanged, i);
            }

            UnityEngine.Events.UnityAction<GameObject> action = new UnityEngine.Events.UnityAction<GameObject>(ToggleValueChanged);
            UnityEditor.Events.UnityEventTools.AddObjectPersistentListener<GameObject>(toggle.onValueChanged, action, gameObject);

            UnityEditor.EditorUtility.SetDirty(this);
        }

        private void Reset()
        {
            Setup();
        }
#endif

        public abstract void ToggleValueChanged(GameObject go);
    }
}
