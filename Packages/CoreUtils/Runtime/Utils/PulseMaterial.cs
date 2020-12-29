using System.Linq;
using UnityEngine;

namespace CoreUtils {
    public class PulseMaterial : MonoBehaviour {
        [SerializeField] private AnimationCurve m_Curve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        [SerializeField] private float m_FadedIn = 1f;
        [SerializeField] private float m_FadeOut;

        private MaterialMod[] m_MaterialMods;
        private static readonly int s_EmissionColor = Shader.PropertyToID("_EmissionColor");

        private void OnEnable() {
            if (m_Curve.postWrapMode != WrapMode.Loop && m_Curve.postWrapMode != WrapMode.PingPong) {
                m_Curve.postWrapMode = WrapMode.PingPong;
            }
            m_MaterialMods = GetComponentsInChildren<Renderer>().SelectMany(r => r.materials.Select(m => new MaterialMod(m))).ToArray();
        }

        private void Update() {
            float alpha = Mathf.LerpUnclamped(m_FadedIn, m_FadeOut, m_Curve.Evaluate(Time.time));
            m_MaterialMods.ForEach(m => m.Update(alpha));
        }

        private void OnDisable() {
            m_MaterialMods.ForEach(m => m.Restore());
        }

        private class MaterialMod {
            private readonly Material m_Material;
            private readonly Color m_OriginalColor;
            private readonly Color m_OriginalEmissionColor;

            public MaterialMod(Material mat) {
                m_Material = mat;
                m_OriginalColor = mat.color;
                m_OriginalEmissionColor = mat.GetColor(s_EmissionColor);
            }

            public void Update(float progress) {
                m_Material.color = m_OriginalColor*progress;
                m_Material.SetColor(s_EmissionColor, m_OriginalEmissionColor*progress);
            }

            public void Restore() {
                m_Material.color = m_OriginalColor;
                m_Material.SetColor(s_EmissionColor, m_OriginalEmissionColor);
            }
        }
    }
}