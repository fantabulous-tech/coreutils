using UnityEngine;

namespace CoreUtils {
    public class Bounce : MonoBehaviour {
        [SerializeField] private float m_Height = 0.25f;
        [SerializeField] private AnimationCurve m_Animation;

        private Vector3 m_StartTransform;

        private void Reset() {
            m_Animation = AnimationCurve.EaseInOut(0, 0, 1, 1);
            m_Animation.postWrapMode = WrapMode.PingPong;
        }

        private void Start() {
            m_StartTransform = transform.localPosition;
        }

        private void LateUpdate() {
            transform.localPosition = m_StartTransform + m_Animation.Evaluate(Time.time)*m_Height*Vector3.up;
        }
    }
}