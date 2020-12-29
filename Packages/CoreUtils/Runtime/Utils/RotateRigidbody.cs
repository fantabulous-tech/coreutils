using UnityEngine;

namespace CoreUtils {
    [RequireComponent(typeof(Rigidbody))]
    public class RotateRigidbody : MonoBehaviour {
        [SerializeField] private Rigidbody m_Rigidbody;
        [SerializeField] private Vector3 m_AngularVelocity = Vector3.up*100;

        private void Start() {
            m_Rigidbody = m_Rigidbody ? m_Rigidbody : GetComponent<Rigidbody>();
        }

        private void Update() {
            m_Rigidbody.angularVelocity = m_AngularVelocity;
        }

        private void Reset() {
            m_Rigidbody = GetComponent<Rigidbody>();
        }
    }
}