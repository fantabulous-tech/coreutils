using UnityEngine;

namespace CoreUtils {
    public class RandomRotate : MonoBehaviour {
        [SerializeField] private Renderer m_Renderer;
        [SerializeField] private float m_MinSpeed = 5;
        [SerializeField] private float m_MaxSpeed = 10;
        [SerializeField] private Vector3 m_AxisOverride = Vector3.zero;

        [SerializeField, ReadOnly] private Vector3 m_Pivot;
        [SerializeField, ReadOnly] private Vector3 m_Axis;
        [SerializeField, ReadOnly] private float m_Speed;

        public Vector3 AxisOverride => m_AxisOverride;
        public Renderer Renderer => m_Renderer;

        private void Reset() {
            m_Renderer = GetComponentInChildren<Renderer>();
            gameObject.isStatic = false;
        }

        private void Start() {
            m_Renderer.gameObject.isStatic = false;
            m_Renderer = m_Renderer ? m_Renderer : GetComponentInChildren<Renderer>();
            m_Pivot = m_Renderer.bounds.center;
            m_Axis = GetAxis();
            m_Speed = Random.Range(m_MinSpeed, m_MaxSpeed);
        }

        private Vector3 GetAxis() {
            return m_AxisOverride.magnitude > 0 ? transform.TransformDirection(m_AxisOverride) : new Vector3(Random.value, Random.value, Random.value);
        }

        private void Update() {
            transform.RotateAround(m_Pivot, m_Axis, m_Speed*Time.deltaTime);
        }
    }
}