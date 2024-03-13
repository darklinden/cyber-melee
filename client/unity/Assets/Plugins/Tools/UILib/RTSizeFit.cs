using UnityEngine;
using UnityEngine.UI;

namespace Wtf
{
    public class RTSizeFit : MonoBehaviour
    {
        protected enum FitType
        {
            None = 0,
            WidthLimit = 1,
            HeightLimit = 2
        }

        [SerializeField] protected FitType _fitType = FitType.None;

        [SerializeField] protected float _maxRateLimitInParent = 1;
        [SerializeField] protected float _minRateLimitInParent = 0;

        [SerializeField][ReadOnly] protected float _widthRateOfParent = 0;
        [SerializeField][ReadOnly] protected float _heightRateOfParent = 0;

        [SerializeField] protected float _ratioWH = 1;

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

        public void RefreshRate()
        {
            Rt.localScale = Vector3.one;

            LayoutRebuilder.ForceRebuildLayoutImmediate(this.transform as RectTransform);

            _ratioWH = Rt.rect.width / Rt.rect.height;

            switch (_fitType)
            {
                case FitType.WidthLimit:
                    {
                        var h = Rt.rect.height;
                        var ph = ParentRt.rect.height;
                        _heightRateOfParent = h / ph;

                        _widthRateOfParent = Rt.rect.width / ParentRt.rect.width;
                    }
                    break;
                case FitType.HeightLimit:
                    {
                        var w = Rt.rect.width;
                        var pw = ParentRt.rect.width;
                        _widthRateOfParent = w / pw;

                        _heightRateOfParent = Rt.rect.height / ParentRt.rect.height;
                    }
                    break;
            }
        }

        protected Vector3 FitWidth()
        {
            if (Rt == null || ParentRt == null)
                return Vector3.one;

            var h = ParentRt.rect.height * _heightRateOfParent;
            var w = h * _ratioWH;

            if (w < ParentRt.rect.width * _minRateLimitInParent)
            {
                w = ParentRt.rect.width * _minRateLimitInParent;
                h = w / _ratioWH;
            }
            else if (w > ParentRt.rect.width * _maxRateLimitInParent)
            {
                w = ParentRt.rect.width * _maxRateLimitInParent;
                h = w / _ratioWH;
            }

            var s = Rt.localScale;
            s.x = w / Rt.rect.width;
            s.y = h / Rt.rect.height;

            return s;
        }

        protected Vector3 FitHeight()
        {
            if (Rt == null || ParentRt == null)
                return Vector3.one;

            var w = ParentRt.rect.width * _widthRateOfParent;
            var h = w / _ratioWH;

            if (h < ParentRt.rect.height * _minRateLimitInParent)
            {
                h = ParentRt.rect.height * _minRateLimitInParent;
                w = h * _ratioWH;
            }
            else if (h > ParentRt.rect.height * _maxRateLimitInParent)
            {
                h = ParentRt.rect.height * _maxRateLimitInParent;
                w = h * _ratioWH;
            }

            var s = Rt.localScale;
            s.x = w / Rt.rect.width;
            s.y = h / Rt.rect.height;

            return s;
        }

        public void Fit()
        {
            switch (_fitType)
            {
                case FitType.WidthLimit:
                    Rt.localScale = FitWidth();
                    _widthRateOfParent = Rt.rect.width * Rt.localScale.x / ParentRt.rect.width;
                    break;
                case FitType.HeightLimit:
                    Rt.localScale = FitHeight();
                    _heightRateOfParent = Rt.rect.height * Rt.localScale.y / ParentRt.rect.height;
                    break;
            }
        }

        public Vector3 FittedScale()
        {
            Vector3 s;
            switch (_fitType)
            {
                case FitType.WidthLimit:
                    s = FitWidth();
                    break;
                case FitType.HeightLimit:
                    s = FitHeight();
                    break;
                default:
                    s = Vector3.one;
                    break;
            }

            return s;
        }

        [ContextMenu("Refresh Rate")]
        public void MenuRefreshRate()
        {
            RefreshRate();

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        [ContextMenu("Fit")]
        public void MenuFit()
        {
            Fit();

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}