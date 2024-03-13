using TMPro;
using UnityEngine;
using Wtf;

namespace UserInterface
{
    public class ToastLoading : MonoBehaviour
    {
        internal TimeLock LoadingLock = new TimeLock("LoadingLock", 8000);
        internal bool IsLoading = false;

        public RectTransform Loading;
        public AnimShowHide LoadingAnim;

        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private float _durationBeforeShow = 0.5f;
        private float _time = -1;
        internal void Show(string text = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                _text.text = i18n.k("Loading ...");
            }
            else
            {
                _text.text = text;
            }

            if (IsLoading)
            {
                // 如果已经开始展示，重置锁时间
                LoadingLock.Lock();
                return;
            }

            IsLoading = true;

            LoadingLock.Lock();
            LoadingAnim.Hide();
            this.gameObject.SetActive(true);
            _time = 0;
        }

        private void HideComplete()
        {
            IsLoading = false;
            this.gameObject.SetActive(false);
        }

        internal void Hide()
        {
            if (!IsLoading)
            {
                Log.W("ToastLoading.Hide: not loading, ignore");
            }

            LoadingLock.Unlock();
            // if (gameObject.activeSelf)
            // {
            //     Log.D("ToastLoading.Hide: active, hideByAnim");
            //     LoadingAnim.AnimHide(HideComplete);
            // }
            // else
            // {
            Log.D("ToastLoading.Hide: inactive, hideDirectly");
            this.gameObject.SetActive(false);
            IsLoading = false;
            // }
        }

        public void OnLoadingClick()
        {
            // Log.D("ToastCanvas.OnLoadingClick");
            if (LoadingLock.IsLocked) return;

            Hide();
        }

        // Update is called once per frame
        private void Update()
        {
            // _time < 0 means not showing
            if (_time < 0) return;

            _time += Time.unscaledDeltaTime;

            if (_time >= _durationBeforeShow)
            {
                _time = -1;
                LoadingAnim.AnimShow();
            }
        }
    }
}