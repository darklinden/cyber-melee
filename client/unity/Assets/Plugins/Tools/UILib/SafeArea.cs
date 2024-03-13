using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Wtf
{
    public class SafeArea : MonoBehaviour
    {
        const float NormalizedScreenLong = 777.0f;
        const float NormalizedScreenShort = 375.0f;

        public struct Insets
        {
            public float Top;
            public float Bottom;
            public float Left;
            public float Right;
        }

        private static bool InsetsEmpty(ref Insets insets)
        {
            return
                Mathf.Abs(insets.Top) < 1
                && Mathf.Abs(insets.Bottom) < 1
                && Mathf.Abs(insets.Left) < 1
                && Mathf.Abs(insets.Right) < 1;
        }

        private static readonly Insets DefaultInsets = new Insets
        {
            Top = 0,
            Bottom = 0,
            Left = 0,
            Right = 0,
        };

        private static readonly Insets LandscapeInsets = new Insets
        {
            Top = 0,
            Bottom = 0,
            Left = 22,
            Right = 22,
        };

        private static readonly Insets PortraitInsets = new Insets
        {
            Top = 44,
            Bottom = 34,
            Left = 0,
            Right = 0,
        };

        [SerializeField] internal bool LayoutOnEnable = true;
        [SerializeField][ReadOnly] internal Insets LastSafeArea = new Insets { Top = 0, Bottom = 0, Left = 0, Right = 0 };

        internal enum EOrientation
        {
            Unknown,
            Landscape,
            Portrait,
        }
        [SerializeField][ReadOnly] internal EOrientation Orientation = EOrientation.Unknown;

        RectTransform m_Rtm = null;
        RectTransform Rtm
        {
            get
            {
                if (m_Rtm == null) { m_Rtm = GetComponent<RectTransform>(); }
                return m_Rtm;
            }
        }
        RectTransform m_PRtm = null;
        RectTransform PRtm
        {
            get
            {
                if (m_PRtm == null) { m_PRtm = m_Rtm != null ? m_Rtm.parent as RectTransform : null; }
                return m_PRtm;
            }
        }


        public Insets CurrentInsets = new Insets { Top = 0, Bottom = 0, Left = 0, Right = 0 };

        [ContextMenu("Refresh SafeArea")]
        public void RefreshSafeArea()
        {
            if (Rtm == null || PRtm == null) return;

            var parentSize = PRtm.rect.size;
            float scaledHeight = 1;
            float scaledWidth = 1;

            if (parentSize.x > parentSize.y)
            {
                // width greater than height assert landscape
                Orientation = EOrientation.Landscape;
                // NormalizedScreenShort as height
                scaledHeight = NormalizedScreenShort;
                scaledWidth = scaledHeight * parentSize.x / parentSize.y;
            }
            else if (parentSize.x <= parentSize.y)
            {
                // assert portrait
                Orientation = EOrientation.Portrait;
                // NormalizedScreenShort as width
                scaledWidth = NormalizedScreenShort;
                scaledHeight = scaledWidth * parentSize.y / parentSize.x;
            }

            Insets insets = new Insets { Top = 0, Bottom = 0, Left = 0, Right = 0 };

            // var screenSafeArea = Screen.safeArea;
            // if ((Orientation == EOrientation.Landscape && screenSafeArea.size.x > screenSafeArea.size.y)
            //     || (Orientation == EOrientation.Portrait && screenSafeArea.size.x <= screenSafeArea.size.y))
            // {
            //     // parent size match screen orientation, can use Screen.safeArea
            //     insets.Left = scaledWidth * screenSafeArea.x / Screen.width;
            //     insets.Top = scaledHeight * screenSafeArea.y / Screen.height;
            //     insets.Right = scaledWidth * (Screen.width - (screenSafeArea.x + screenSafeArea.width)) / Screen.width;
            //     insets.Bottom = scaledHeight * (Screen.height - (screenSafeArea.y + screenSafeArea.height)) / Screen.height;
            // }

            if (InsetsEmpty(ref insets))
            {
                // use percent calc
                switch (Orientation)
                {
                    case EOrientation.Landscape:
                        insets = (parentSize.x / parentSize.y) >= (NormalizedScreenLong / NormalizedScreenShort) ? LandscapeInsets : DefaultInsets;
                        break;
                    case EOrientation.Portrait:
                        insets = (parentSize.y / parentSize.x) >= (NormalizedScreenLong / NormalizedScreenShort) ? PortraitInsets : DefaultInsets;
                        break;
                }
            }

            // Applly
            float left = Mathf.FloorToInt(parentSize.x * insets.Left / scaledWidth);
            float top = Mathf.FloorToInt(parentSize.y * insets.Top / scaledHeight);
            float right = Mathf.FloorToInt(parentSize.x * insets.Right / scaledWidth);
            float bottom = Mathf.FloorToInt(parentSize.y * insets.Bottom / scaledHeight);

            CurrentInsets = new Insets
            {
                Top = Mathf.Abs(top),
                Bottom = Mathf.Abs(bottom),
                Left = Mathf.Abs(left),
                Right = Mathf.Abs(right),
            };

            Rtm.anchorMin = Vector2.zero;
            Rtm.anchorMax = Vector2.one;
            Rtm.localScale = Vector3.one;
            Rtm.offsetMin = new Vector2(left, bottom);
            Rtm.offsetMax = new Vector2(-right, -top);
        }

        private void OnEnable()
        {
            if (LayoutOnEnable)
            {
                StartCoroutine(DelayAndRefresh());
            }
        }

        private IEnumerator DelayAndRefresh()
        {
            yield return null;
            yield return null;
            yield return null;
            RefreshSafeArea();
        }
    }
}