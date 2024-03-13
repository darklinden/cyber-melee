using UnityEngine;
using UnityEngine.UI;

namespace Wtf
{
    public abstract class AbstractUIButton : MonoBehaviour
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

            UnityEditor.EditorUtility.SetDirty(this);
        }

        protected void Reset()
        {
            Setup();
        }
#endif

        public abstract void OnButtonClicked(GameObject go);
    }
}
