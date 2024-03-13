using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Wtf
{
    public class ScreenCapture : MonoBehaviour
    {
#if UNITY_EDITOR
        private bool _isCapture = false;

        void Update()
        {
            var isCtrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            var isShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            var isAlt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            var isC = Input.GetKey(KeyCode.C);
            // Debug.Log($"ScreenCapture isCtrl: {isCtrl} isShift: {isShift} isAlt: {isAlt} isC: {isC}");
            if (isCtrl && isShift && isAlt && isC)
            {
                _isCapture = true;
            }
            else
            {
                if (_isCapture)
                {
                    _isCapture = false;

                    var assetsPath = Application.dataPath;
                    var projectPath = Path.GetDirectoryName(assetsPath);
                    var capturePath = Path.Combine(projectPath, "Capture");
                    if (!Directory.Exists(capturePath))
                    {
                        Directory.CreateDirectory(capturePath);
                    }
                    var captureName = $"Capture_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
                    UnityEngine.ScreenCapture.CaptureScreenshot(Path.Combine(capturePath, captureName));

                    Debug.LogError($"ScreenCaptured! : {captureName}");
                }
            }
        }
#endif
    }
}
