using UnityEngine;
using UnityEngine.UI;

namespace CoreUtils {
    [RequireComponent(typeof(Graphic))]
    public class Pulse : MonoBehaviour {
        [SerializeField] private AnimationCurve m_Curve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        [SerializeField] private float m_FadedIn = 1f;
        [SerializeField] private float m_FadeOut;

        private Graphic[] m_Graphics;

        private void OnEnable() {
            m_Graphics = GetComponentsInChildren<Graphic>();
            if (m_Curve.postWrapMode != WrapMode.Loop && m_Curve.postWrapMode != WrapMode.PingPong) {
                m_Curve.postWrapMode = WrapMode.PingPong;
            }
        }

        private void OnDisable() {
            m_Graphics.ForEach(g => g.color = new Color(g.color.r, g.color.g, g.color.b, m_FadedIn));
        }

        private void Update() {
            float alpha = Mathf.LerpUnclamped(m_FadedIn, m_FadeOut, m_Curve.Evaluate(Time.time));
            m_Graphics.ForEach(g => g.color = new Color(g.color.r, g.color.g, g.color.b, alpha));
        }
    }
}