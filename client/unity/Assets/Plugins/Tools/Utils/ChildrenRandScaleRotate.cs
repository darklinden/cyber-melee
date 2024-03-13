using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Wtf
{
    public class ChildrenRandScaleRotate : MonoBehaviour
    {
#if UNITY_EDITOR

        [SerializeField] private float _minScale = 0.5f;
        [SerializeField] private float _maxScale = 1.5f;
        [SerializeField] private float _minRotate = 0f;
        [SerializeField] private float _maxRotate = 359f;
        [SerializeField] private RectTransform[] _children = null;

        [ContextMenu("Reset Children")]
        private void ResetChildren()
        {
            _children = transform.childCount > 0 ? new RectTransform[transform.childCount] : null;
            for (int i = 0; i < transform.childCount; i++)
            {
                _children[i] = transform.GetChild(i) as RectTransform;
            }
        }

        private bool _isCapture = false;

        void Update()
        {
            var isCtrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            var isShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            var isAlt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            var isR = Input.GetKey(KeyCode.R);
            // Debug.Log($"ScreenCapture isCtrl: {isCtrl} isShift: {isShift} isAlt: {isAlt} isC: {isC}");
            if (isCtrl && isShift && isAlt && isR)
            {
                _isCapture = true;
            }
            else
            {
                if (_isCapture)
                {
                    _isCapture = false;

                    foreach (var child in _children)
                    {
                        var scale = Random.Range(_minScale, _maxScale);
                        child.localScale = new Vector3(scale, scale, scale);

                        var rotate = Random.Range(_minRotate, _maxRotate);
                        child.localRotation = Quaternion.Euler(0, 0, rotate);
                    }
                }
            }
        }
#endif
    }
}
