using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wtf
{
    public class InfinityStepRotate : MonoBehaviour
    {
        public float Step = 10f;
        public Vector3 Axis = Vector3.back;
        public float Interval = 0.1f;

        private float _timer = 0f;
        // Update is called once per frame
        void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < Interval) return;
            _timer = 0f;
            transform.Rotate(Axis, Step);
        }
    }
}
