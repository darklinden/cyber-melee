using UnityEngine;

namespace Wtf
{
    public class Billboard : MonoBehaviour
    {
        [SerializeField] private Camera _Camera;
        public Camera Camera { get => _Camera; set => _Camera = value; }
        [SerializeField] private Transform _CameraTm;
        public Transform CameraTm { get => _CameraTm; set => _CameraTm = value; }

        public bool Reversed;

        public bool ManualUpdate;

        [SerializeField] private Transform _Tm;
        public Transform Tm { get => _Tm; private set => _Tm = value; }

        protected void Awake()
        {
            if (Tm == null)
            {
                Tm = transform;
            }
        }

        protected void LateUpdate()
        {
            if (!ManualUpdate)
            {
                UpdateBillboard();
            }
        }

        [ContextMenu("Update Billboard")]
        public void UpdateBillboard()
        {
            if (Camera == null)
            {
                Camera = Camera.main;
                CameraTm = Camera.transform;
            }

            if (Camera != null)
            {
                if (Reversed)
                {
                    Vector3 vector = CameraTm.position - Tm.position;
                    Tm.LookAt(Tm.position - vector, CameraTm.up);
                }
                else
                {
                    Tm.LookAt(CameraTm.position);
                }
            }
        }
    }
}