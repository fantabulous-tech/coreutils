using UnityEngine;

namespace CoreUtils {
    public class RandomInitialRotation : MonoBehaviour {
        [SerializeField, Range(0, 360)] private float m_Min;
        [SerializeField, Range(0, 360)] private float m_Max = 360;
        [SerializeField] private bool m_Local = true;
        [SerializeField] private Vector3 m_Axis = Vector3.up;

        private void OnEnable() {
            Quaternion rotation =  Quaternion.AngleAxis(Random.Range(m_Min, m_Max), m_Axis);

            if (m_Local) {
                transform.localRotation = rotation;
            } else {
                transform.rotation = rotation;
            }
        }
    }
}