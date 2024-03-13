using UnityEngine;
using UnityEngine.UI;

namespace Wtf
{
    public abstract partial class CommonPanel
    {
        public virtual void AOpen()
        {
            AOpen(false);
        }

        public virtual void AOpen(bool immdiatly = false)
        {
            if (IsAffecting) return;
            IsAffecting = true;

            IsShowing = false;

            OnOpenStart();

            switch (_closeType)
            {
                case CommonPanelCloseType.Active:
                    gameObject.SetActive(true);
                    break;
                case CommonPanelCloseType.Alpha:
                    _canvasGroup.alpha = 1;
                    _canvasGroup.interactable = true;
                    _canvasGroup.blocksRaycasts = true;
                    break;
            }

            if (immdiatly)
            {
                OpenComplete();
            }
            else
            {
                if (_animRectSizeFit != null)
                {
                    _animRectSizeFit.AnimEnable(OpenComplete);
                }
                else
                {
                    this.DelayDo(OpenComplete);
                }
            }
        }

        private void OnEnable()
        {
            var rt = this.transform as RectTransform;
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);
            rt.localScale = new Vector3(1, 1, 1);

            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

            if (this._animRectSizeFit != null)
            {
                _animRectSizeFit.BlockTouch();
            }
        }

        private void OpenComplete()
        {
            IsShowing = true;

            OnOpenComplete();
        }
    }
}