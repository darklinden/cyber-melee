using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Wtf
{
    public class CallEvents : MonoBehaviour
    {
        // Event delegates triggered on click.
        [FormerlySerializedAs("onClick")]
        [SerializeField]
        private UnityEvent m_OnCall = new UnityEvent();

        public void Call()
        {
            m_OnCall.Invoke();
        }
    }
};