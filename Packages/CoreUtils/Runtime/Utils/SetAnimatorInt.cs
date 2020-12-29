using UnityEngine;

namespace CoreUtils {
    public class SetAnimatorInt : SetAnimatorBase {
        [SerializeField] private string m_ParameterName;

        public void SetTo(int value) {
            SetInternal(() => m_Animator.SetInteger(m_ParameterName, value));
        }
    }
}