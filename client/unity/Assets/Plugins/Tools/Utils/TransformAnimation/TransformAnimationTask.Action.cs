using UnityEngine;

namespace Wtf
{
    public partial class TransformAnimationTask
    {
        private Easing.Function m_animation_action_easing = Easing.Function.LINEAR;
        private System.Action<float> m_animation_action = null;

        public void action(System.Action<float> animationAction, Easing.Function easing = Easing.Function.LINEAR)
        {
            if (m_timer > 0f)
            {
                Debug.LogError("Cannot modify task after it has been started!");
                return;
            }
            m_animation_action_easing = easing;
            m_animation_action = animationAction;
        }
    }
}