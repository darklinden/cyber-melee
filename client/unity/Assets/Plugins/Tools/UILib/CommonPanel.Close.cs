using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace Wtf
{
    public abstract partial class CommonPanel
    {
        public virtual void AClose()
        {
            AClose(false);
        }

        public virtual void AClose(bool immdiatly = false)
        {
            if (!IsShowing) return;
            IsShowing = false;

            OnCloseStart();

            if (immdiatly)
            {
                CloseComplete();
            }
            else
            {
                if (_animRectSizeFit != null)
                {
                    _animRectSizeFit.AnimDisable(CloseComplete);
                }
                else
                {
                    this.DelayDo(CloseComplete);
                }
            }
        }

        private void CloseComplete()
        {
            // do close
            switch (_closeType)
            {
                case CommonPanelCloseType.Active:
                    gameObject.SetActive(false);
                    break;
                case CommonPanelCloseType.Alpha:
                    _canvasGroup.alpha = 0;
                    _canvasGroup.interactable = false;
                    _canvasGroup.blocksRaycasts = false;
                    break;
            }

            _panelClosedCall?.Invoke(ReleaseOnClose);
            if (ReleaseOnClose)
            {
                if (!Addressables.ReleaseInstance(gameObject)) UnityEngine.Object.Destroy(gameObject);
            }

            OnCloseComplete();
            IsAffecting = false;
        }
    }
}