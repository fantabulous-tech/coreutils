using UnityEngine;

namespace CoreUtils {
    public class DistributeChildren : MonoBehaviour {
        [SerializeField] private float m_Distance = 0.5f;
        [SerializeField] private Vector3 m_Axis = Vector3.right;
        [SerializeField] private bool m_UpdateInEditor;

        private int m_LastCount;
        private float m_LastDistance;
        private Vector3 m_LastAxis;

        public bool HasChanged => this && m_LastCount != transform.childCount || !m_Distance.Approximately(m_LastDistance) || !m_LastAxis.Approximately(m_Axis);

        private void Update() {
            if (!HasChanged) {
                return;
            }

            Distribute();
        }

        public void Distribute() {
            m_LastCount = transform.childCount;
            m_LastDistance = m_Distance;
            m_LastAxis = m_Axis;

            float halfDistance = m_Distance*(m_LastCount - 1)/2;

            for (int i = 0; i < m_LastCount; i++) {
                Transform child = transform.GetChild(i);
                Vector3 axis = m_Axis.normalized;
                child.localPosition = m_Distance*i*axis - axis*halfDistance;
            }
        }

        private void OnValidate() {
            if (m_UpdateInEditor) {
                Distribute();
            }
        }
    }
}