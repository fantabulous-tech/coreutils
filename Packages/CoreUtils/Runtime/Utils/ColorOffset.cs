using UnityEngine;

namespace CoreUtils {
    public class ColorOffset : MonoBehaviour {
        [SerializeField] private Renderer m_Source;
        [SerializeField, Range(0, 1)] private float m_ColorFade = 0.5f;
        [SerializeField] private Renderer[] m_Targets;

        private void Reset() {
            m_Targets = GetComponentsInChildren<Renderer>();
        }

        private void LateUpdate() {
            if (!m_Source || !m_Source.sharedMaterial || m_Targets.Length == 0) {
                return;
            }

            Color c = m_Source.material.color;

            foreach (Renderer r in m_Targets) {
                if (r.sharedMaterial != m_Source.sharedMaterial) {
                    r.sharedMaterial = m_Source.sharedMaterial;
                }

                r.material.color = new Color(c.r, c.g, c.b, c.a*m_ColorFade);
            }
        }
    }
}