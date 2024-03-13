using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif

namespace Wtf
{
    [RequireComponent(typeof(Image)), DisallowMultipleComponent]
    public abstract partial class CommonPanel : MonoBehaviour
    {
        // 偷懒 addressable path
        public static string PanelPath { get; }

        // 自动设置背景时的颜色
        private static readonly Color BgColor = new Color(0, 0, 0, 150.0f / 255.0f);

        // 关闭时使用 gameObject.SetActive() 还是 _canvasGroup.alpha
        [SerializeField] protected CommonPanelCloseType _closeType = CommonPanelCloseType.Alpha;

        // 是否在开关时使用 tween 动画
        [SerializeField] protected bool _autoAnimateShow = true;

        // 点击背景是否关闭 默认 true 点击关闭
        [SerializeField] protected bool _clickBgClose = true;

        // 自动缩放自适应屏幕控件
        [SerializeField] protected RTSizeFitPanel _animRectSizeFit = null;

        // 改变 alpha 值时使用的 canvasGroup
        [SerializeField] protected CanvasGroup _canvasGroup = null;

        // 背景图片
        [SerializeField] protected Image _backGroundImage = null;
        // 背景点击按钮
        [SerializeField] protected Button _backGroundCloseBtn = null;

        // 是否正在显示 开启动画执行后 true 关闭动画执行前 false
        public bool IsShowing { get; protected set; }

        // 是否正在影响界面 开启动画执行前 true 关闭动画执行后 false
        public bool IsAffecting { get; protected set; }

        private bool PredicateForAwaitClose()
        {
            return !IsAffecting;
        }

        public async UniTask AwaitClose()
        {
            await UniTask.WaitUntil(PredicateForAwaitClose);
        }

        // 关闭完成回调
        private Action<bool> _panelClosedCall = null;
        public void SetClosedCallback(Action<bool> callback)
        {
            _panelClosedCall = callback;
        }

        // 分组 用于快速判断是否有同组面板在显示
        public virtual int PanelGroup => 0;

        // 是否在关闭时释放资源 默认 false 只是隐藏当前面板 在切换场景时会释放
        public virtual bool ReleaseOnClose => false;

        /// <summary>
        /// 面板开始打开时的回调 虚函数为空 可不用重写
        /// </summary>
        protected virtual void OnOpenStart() { }

        /// <summary>
        /// 面板完全打开时的回调 虚函数为空 可不用重写
        /// </summary>
        protected virtual void OnOpenComplete() { }

        /// <summary>
        /// 面板开始关闭时的回调 虚函数为空 可不用重写
        /// </summary>
        protected virtual void OnCloseStart() { }

        /// <summary>
        /// 面板完全关闭时的回调 虚函数为空 可不用重写
        /// </summary>
        protected virtual void OnCloseComplete() { }

#if UNITY_EDITOR
        [ContextMenu("Auto Set Background")]
        protected void AutoTransparentBg()
        {
            _backGroundImage = gameObject.AddOrGetComponent<Image>();

            if (_backGroundImage.sprite == null)
            {
                _backGroundImage.sprite = null;
                _backGroundImage.rectTransform.offsetMin = new Vector2(0, 0);
                _backGroundImage.rectTransform.offsetMax = new Vector2(0, 0);
                _backGroundImage.rectTransform.localScale = new Vector3(1, 1, 1);
                _backGroundImage.color = BgColor;
            }

            UnityEditor.EditorUtility.SetDirty(this);
        }

        [ContextMenu("Auto Set Properties")]
        protected void AutoSetProperties()
        {
            _closeType = CommonPanelCloseType.Alpha;

            _canvasGroup = gameObject.AddOrGetComponent<CanvasGroup>();
            _animRectSizeFit = GetComponentInChildren<RTSizeFitPanel>();

            _backGroundCloseBtn = gameObject.AddOrGetComponent<UnityEngine.UI.Button>();
            if (_backGroundCloseBtn == null) _backGroundCloseBtn = gameObject.AddComponent<UnityEngine.UI.Button>();
            _backGroundCloseBtn.transition = UnityEngine.UI.Selectable.Transition.None;

            var persistentEventCount = _backGroundCloseBtn.onClick.GetPersistentEventCount();
            if (persistentEventCount > 0)
            {
                for (int i = persistentEventCount - 1; i >= 0; i--)
                    UnityEventTools.RemovePersistentListener(_backGroundCloseBtn.onClick, i);
            }

            UnityAction<GameObject> action = new UnityAction<GameObject>(OnBgClicked);
            UnityEventTools.AddObjectPersistentListener<GameObject>(_backGroundCloseBtn.onClick, action, gameObject);

            UnityEditor.EditorUtility.SetDirty(this);
        }

#endif

        private void OnBgClicked(GameObject go)
        {
            if (_clickBgClose) AClose();
        }

        private Dictionary<string, bool> _historyExists = null;
        protected bool FirstTimeEnable(string key = "default")
        {
            bool x = false;
            if (_historyExists == null) _historyExists = new Dictionary<string, bool>();
            if (!_historyExists.TryGetValue(key, out x))
            {
                x = false;
            }

            if (!x)
            {
                _historyExists[key] = true;
            }

            return !x;
        }
    }
}