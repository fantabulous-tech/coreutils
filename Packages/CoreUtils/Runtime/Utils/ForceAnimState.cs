using UnityEngine;

namespace CoreUtils {
    [ExecuteInEditMode]
    public class ForceAnimState : MonoBehaviour {
        [SerializeField] private AnimationClip m_Animation;
        [SerializeField] private float m_Time;

        private void LateUpdate() {
            if (CanForceAnim) {
                m_Animation.SampleAnimation(gameObject, MathUtils.Mod(m_Time, m_Animation.length));
            }
        }

        private bool CanForceAnim => m_Animation;
    }
}