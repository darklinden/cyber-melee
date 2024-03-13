using UnityEngine;
using UnityEngine.UI;

namespace Wtf
{
    [RequireComponent(typeof(Image))]
    public class ImageSizeFit : MonoBehaviour
    {
        protected enum FitType
        {
            None = 0,
            FitWidth = 1,
            FitHeight = 2,
            FitBoth = 3
        }

        [SerializeField] protected FitType _fitType = FitType.None;
        [SerializeField] protected float _rateOfParent = 1;
        [SerializeField] protected Image _image = null;

        public Image Image
        {
            get
            {
                if (_image == null)
                {
                    _image = GetComponent<Image>();
                }

                return _image;
            }
        }

        protected RectTransform m_Rt;
        public RectTransform Rt
        {
            get
            {
                if (m_Rt == null)
                {
                    m_Rt = GetComponent<RectTransform>();
                }

                return m_Rt;
            }
        }

        protected RectTransform m_ParentRt;
        public RectTransform ParentRt
        {
            get
            {
                if (m_ParentRt == null)
                {
                    m_ParentRt = Rt.parent.GetComponent<RectTransform>();
                }

                return m_ParentRt;
            }
        }

        protected bool FitWidth(float whRatio, out float w, out float h)
        {
            w = 0;
            h = 0;
            if (Rt == null || ParentRt == null)
                return false;

            w = ParentRt.rect.width * _rateOfParent;
            h = w / whRatio;

            return true;
        }

        protected bool FitHeight(float whRatio, out float w, out float h)
        {
            w = 0;
            h = 0;
            if (Rt == null || ParentRt == null)
                return false;

            h = ParentRt.rect.height * _rateOfParent;
            w = h * whRatio;

            return true;
        }

        public void Fit()
        {
            var sprite = Image.sprite;
            if (sprite == null)
            {
                Log.D("ImageSizeFit.Fit: sprite is null");
                return;
            }

            float whRatio = sprite.rect.width / sprite.rect.height;

            switch (_fitType)
            {
                case FitType.FitWidth:
                    {
                        if (FitWidth(whRatio, out float w, out float h))
                        {
                            Rt.sizeDelta = new Vector2(w, h);
                        }
                    }
                    break;
                case FitType.FitHeight:
                    {
                        if (FitHeight(whRatio, out float w, out float h))
                        {
                            Rt.sizeDelta = new Vector2(w, h);
                        }
                    }
                    break;
                case FitType.FitBoth:
                    {
                        var fw = FitWidth(whRatio, out float ww, out float wh);
                        var fh = FitHeight(whRatio, out float hw, out float hh);
                        if (fw && fh)
                        {
                            if (ww > hw)
                            {
                                Rt.sizeDelta = new Vector2(hw, hh);
                            }
                            else
                            {
                                Rt.sizeDelta = new Vector2(ww, wh);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void Reset()
        {
            _image = GetComponent<Image>();
        }

        [ContextMenu("Fit")]
        public void MenuFit()
        {
            Fit();
        }

        [ContextMenu("Fit Parent Size")]
        public void MenuFitParentSize()
        {
            if (Rt == null || ParentRt == null)
                return;

            Rt.sizeDelta = ParentRt.sizeDelta;
        }
    }
}