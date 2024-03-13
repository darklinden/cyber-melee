using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

namespace Wtf
{
    public class ToggleSwitch : MonoBehaviour
    {
        private RectTransform _rt;
        public RectTransform Rt => _rt != null ? _rt : (_rt = GetComponent<RectTransform>());

        public enum EState
        {
            Unknown, // 用来处理初始状态
            Disabled,
            Off,
            On,
        }

        [SerializeField] protected EState _state = EState.Unknown;

        public Action<EState> OnStateChanged = null;

        public EState State
        {
            get { return _state; }
            set
            {
                if (_state == value) return;
                _state = value;
                UpdateUI();
                OnStateChanged?.Invoke(_state);
            }
        }

        protected virtual void UpdateUI() { }

        public bool IsOn
        {
            get { return _state == EState.On; }
            set
            {
                if (_state == EState.Disabled) return;
                if (value)
                    State = EState.On;
                else
                    State = EState.Off;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            State = _state;
        }
#endif

        [SerializeField] internal ToggleSwitchGroup ToggleGroup;
    }
}