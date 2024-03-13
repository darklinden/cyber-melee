using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace UserInterface
{
    public static class Toast
    {
        static string CANVAS_PATH => $"Assets/Addrs/{i18n.Locale}/Toast/ToastCanvas.prefab";
        private static bool _initialized = false;
        private static ToastCanvas _toastCanvas;
        public static void Initialize()
        {
            AsyncInitialize().Forget();
        }

        public static async UniTask AsyncInitialize()
        {
            if (_initialized) return;
            _initialized = true;

            var canvas = await Addressables.InstantiateAsync(CANVAS_PATH);
            if (canvas == null)
            {
                Log.E("ToastCanvas is null");
                return;
            }

            _toastCanvas = canvas.GetComponent<ToastCanvas>();
            if (_toastCanvas == null)
            {
                Log.E("ToastCanvas is null");
                return;
            }
        }

        public static void Show(string text)
        {
            if (_initialized == false)
            {
                throw new System.Exception("Toast is not initialized");
            }

            if (string.IsNullOrEmpty(text)) return;

            if (_toastCanvas.ToastPool.Spawned.Count > 0)
            {
                foreach (var item in _toastCanvas.ToastPool.Spawned)
                {
                    var toast = item.GetComponent<ToastImpl>();
                    toast.Move();
                }
            }

            var rtInstance = _toastCanvas.ToastPool.Get();
            if (rtInstance == null) return;

            var instance = rtInstance.GetComponent<ToastImpl>();
            instance.Rt.SetAsLastSibling();
            instance.Show(text);
        }

        public static void Return(ToastImpl instance)
        {
            if (_toastCanvas.ToastPool != null)
            {
                _toastCanvas.ToastPool.Return(instance);
            }
            else
            {
                GameObject.Destroy(instance.gameObject);
            }
        }

        public static void Deinitialize()
        {
            _initialized = false;
            if (_toastCanvas != null)
            {
                _toastCanvas.Deinitialize();
            }
        }

        #region Toast Loading

        public static void ShowLoading(string text = null)
        {
            Log.D("Toast.ShowLoading");

            if (_initialized == false)
            {
                throw new System.Exception("Toast is not initialized");
            }

            _toastCanvas.ToastLoading.Show(text);
        }

        public static void HideLoading()
        {
            Log.D("Toast.HideLoading");

            if (_initialized == false)
            {
                throw new System.Exception("Toast is not initialized");
            }

            _toastCanvas.ToastLoading.Hide();
        }

        #endregion
    }
}