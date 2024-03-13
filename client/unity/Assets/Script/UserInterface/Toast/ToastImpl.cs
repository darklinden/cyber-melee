using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UserInterface
{
    public class ToastImpl : MonoBehaviour
    {
        private RectTransform _rt;
        public RectTransform Rt => _rt != null ? _rt : _rt = this.GetComponent<RectTransform>();

        const float _duration = 1f;
        const float _hideDuration = 0.5f;
        const float _moveSpeed = 100f;
        const float _jumpSize = 80f;

        [SerializeField] private Image _img;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Color _imgColor = new Color(0, 0, 0, 180.0f / 255.0f);
        [SerializeField] private Color _textColor = new Color(1, 1, 1, 1);

        private float _timeLast = float.MaxValue;
        internal void Show(string text)
        {
            _text.text = text;
            _timeLast = _duration + _hideDuration;

            Rt.offsetMin = new Vector2(0.5f, 0.5f);
            Rt.offsetMax = new Vector2(0.5f, 0.5f);
            Rt.pivot = new Vector2(0.5f, 0.5f);
            Rt.localScale = new Vector3(1, 1, 1);

            _img.color = _imgColor;
            _text.color = _textColor;

            _img.rectTransform.anchoredPosition = new Vector2(0, 0);
        }

        // Update is called once per frame
        private void FixedUpdate()
        {
            if (_timeLast < 0) return;

            var dt = Time.fixedUnscaledDeltaTime;

            if (_timeLast > 0)
                _timeLast -= dt;

            if (_timeLast > 0 && _timeLast < _hideDuration)
            {
                var ci = _imgColor;
                ci.a *= _timeLast / _hideDuration;
                _img.color = ci;

                var ct = _textColor;
                ct.a *= _timeLast / _hideDuration;
                _text.color = ct;

                var pos = Rt.anchoredPosition;
                pos.y += _moveSpeed * dt;
                Rt.anchoredPosition = pos;
            }

            if (_timeLast < 0)
            {
                _timeLast = -1;
                Toast.Return(this);
            }
        }

        public void Move()
        {
            if (_timeLast < 0) return;

            var pos = Rt.anchoredPosition;
            pos.y += _jumpSize;
            Rt.anchoredPosition = pos;
        }
    }
}