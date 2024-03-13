using System;
using System.Collections;
using UnityEngine;
using UnityEngine.U2D;

#if PixelPerfectCamera

namespace Wtf
{
    public class SafePixelPerfectCamera : MonoBehaviour
    {
        [SerializeField] internal PixelPerfectCamera PixelPerfectCamera;
        [SerializeField] internal Camera Camera;
        [SerializeField] internal int PixelPerfectAligh = 160;

        internal enum EOrientation
        {
            Unknown,
            Landscape,
            Portrait,
        }
        [SerializeField][ReadOnly] internal EOrientation Orientation = EOrientation.Unknown;

        private void Reset()
        {
            PixelPerfectCamera = GetComponent<PixelPerfectCamera>();
            Camera = GetComponent<Camera>();
        }

        [ContextMenu("Refresh Safe Camera")]
        public void RefreshSafeCamera()
        {
            if (Camera == null || PixelPerfectCamera == null)
            {
                PixelPerfectCamera = GetComponent<PixelPerfectCamera>();
                Camera = GetComponent<Camera>();
            }

            var px = Camera.pixelWidth;
            var py = Camera.pixelHeight;

            if (px > py)
            {
                // width greater than height assert landscape
                Orientation = EOrientation.Landscape;
            }
            else if (px <= py)
            {
                // assert portrait
                Orientation = EOrientation.Portrait;
            }

            Log.D("RefreshSafeCamera", Orientation, px, py);

            switch (Orientation)
            {
                case EOrientation.Landscape:
                    PixelPerfectCamera.refResolutionY = PixelPerfectAligh;
                    PixelPerfectCamera.refResolutionX = PixelPerfectAligh * px / py;
                    break;
                case EOrientation.Portrait:
                    PixelPerfectCamera.refResolutionX = PixelPerfectAligh;
                    PixelPerfectCamera.refResolutionY = PixelPerfectAligh * py / px;
                    break;
                default:
                    break;
            }

        }

        private void Start()
        {
            RefreshSafeCamera();
        }
    }
}

#endif