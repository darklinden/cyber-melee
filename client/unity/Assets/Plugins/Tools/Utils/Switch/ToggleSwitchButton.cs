using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

namespace Wtf
{
    [DisallowMultipleComponent, RequireComponent(typeof(Image)), RequireComponent(typeof(Button))]
    public class ToggleSwitchButton : ToggleSwitch
    {
        public Button button;

#if UNITY_EDITOR
        [ContextMenu("Setup")]
        protected void Setup()
        {
            if (button == null)
            {
                button = this.GetComponent<Button>();
                if (button.targetGraphic == null)
                    button.targetGraphic = this.gameObject.AddComponent<Image>();
            }

            var persistentEventCount = button.onClick.GetPersistentEventCount();
            if (persistentEventCount > 0)
            {
                for (int i = persistentEventCount - 1; i >= 0; i--)
                    UnityEditor.Events.UnityEventTools.RemovePersistentListener(button.onClick, i);
            }

            UnityEngine.Events.UnityAction<GameObject> action = new UnityEngine.Events.UnityAction<GameObject>(OnButtonClicked);
            UnityEditor.Events.UnityEventTools.AddObjectPersistentListener<GameObject>(button.onClick, action, gameObject);
        }

        protected void Reset()
        {
            Setup();
        }
#endif

        public virtual void OnButtonClicked(GameObject go)
        {
            if (ToggleGroup != null)
            {
                ToggleGroup.OnItemClicked(this);
            }
            else
            {
                IsOn = !IsOn;
            }
        }
    }
}