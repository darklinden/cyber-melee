using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using TMPro;
using Wtf;
using System;

namespace UserInterface
{
    public class ToastCanvas : MonoBehaviour
    {
        string PREFAB_PATH => $"Assets/Addrs/{i18n.Locale}/Toast/Toast.prefab";

        public Canvas Canvas;

        public GameObjectPool ToastPool;

        public ToastLoading ToastLoading;

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);

            AsyncInitialize().Forget();
        }

        private async UniTask AsyncInitialize()
        {
            var prefab = await LoadUtil.AsyncLoad<GameObject>(PREFAB_PATH);
            if (prefab == null) return;

            ToastPool.Initialize(prefab, PREFAB_PATH);
            ToastPool.HideType = GameObjectPoolHideType.UIAlpha;
            await ToastPool.PrewarmAsync(3);
        }

        public void Deinitialize()
        {
            ToastPool.Deinitialize();
        }
    }
}