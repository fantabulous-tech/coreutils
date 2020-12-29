using UnityEngine;

namespace CoreUtils {
    public class LookAtTarget : MonoBehaviour {
        [SerializeField] private Transform m_Target;
        [SerializeField] private bool m_Flip;
        [SerializeField] private Vector3 m_Up = Vector3.up;
        [SerializeField] private Vector3 m_RotationOffset;

        private Transform Target => m_Target ? m_Target : UnityUtils.CameraTransform;

        private void LateUpdate() {
            UpdateLook();
        }

        public void UpdateLook() {
            if (m_Target) {
                if (m_Up.magnitude > 0) {
                    transform.LookAt(m_Target, m_Up);
                } else {
                    transform.LookAt(m_Target);
                }

                transform.rotation *= Quaternion.Euler(m_RotationOffset);

                if (m_Flip) {
                    transform.rotation *= Quaternion.AngleAxis(180, transform.up);
                }
            }
        }
    }
}