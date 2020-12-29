using UnityEngine;

namespace CoreUtils {
    public class OnStateChangeEvents : StateMachineBehaviour {
        [SerializeField] private string m_SendEventOnEnter;
        [SerializeField] private string m_SendEventOnExit;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (!m_SendEventOnEnter.IsNullOrEmpty()) {
                animator.SendMessage("SendEvent", m_SendEventOnEnter);
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (!m_SendEventOnExit.IsNullOrEmpty()) {
                animator.SendMessage("SendEvent", m_SendEventOnExit);
            }
        }
    }
}