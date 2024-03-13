using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wtf
{
    public class ToggleSwitchGroup : MonoBehaviour
    {
        public int CurrentIndex;
        public List<ToggleSwitch> Items;

        // call on changed 
        protected System.Action<int> m_OnValueChangedCallback;
        public void SetOnValueChangedCallback(System.Action<int> callback)
        {
            m_OnValueChangedCallback = callback;
        }

        // return true to allow change
        protected System.Func<int, bool> m_OnValueWillChangedCallback;
        public void SetOnValueWillChangedCallback(System.Func<int, bool> callback)
        {
            m_OnValueWillChangedCallback = callback;
        }

        internal virtual void OnItemClicked(ToggleSwitch item)
        {
            var index = Items.IndexOf(item);
            if (index >= 0)
            {
                if (m_OnValueWillChangedCallback != null)
                {
                    if (!m_OnValueWillChangedCallback(index)) return;
                }

                SelectItem(index);
            }
        }

        protected void Reset()
        {
            var items = GetComponentsInChildren<ToggleSwitch>();
            if (Items == null) Items = new List<ToggleSwitch>();
            Items.Clear();
            Items.AddRange(items);
            foreach (var item in Items)
            {
                item.ToggleGroup = this;
            }
        }

        public virtual void SelectItem(int index, bool callback = true)
        {
            if (Items == null) return;

            if (index < 0 || index >= Items.Count) return;

            for (var i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                if (item != null)
                    item.IsOn = i == index;
            }

            CurrentIndex = index;

            if (callback)
                m_OnValueChangedCallback?.Invoke(index);
        }
    }
}