using UnityEngine;

namespace CoreUtils.UI {
    [ExecuteInEditMode]
    public class LockAspectRatio : MonoBehaviour {
        [SerializeField] private Vector2 m_AspectRatio = new Vector2(16, 9);

        private Camera m_Cam;
        private float m_LastAspect;

        private float WantedAspectRatio => m_AspectRatio.x/m_AspectRatio.y;

        private void Awake() {
            m_Cam = GetComponent<Camera>();

            if (!m_Cam) {
                m_Cam = Camera.main;
            }

            if (!m_Cam) {
                Debug.LogError("No camera available");
            }
        }

        private void Update() {
            if (!m_Cam) {
                return;
            }

            float currentAspect = (float) Screen.width/Screen.height;

            if (currentAspect.Approximately(m_LastAspect, 0.01f)) {
                return;
            }

            m_LastAspect = currentAspect;

            if (m_LastAspect.Approximately(WantedAspectRatio, 0.01f)) {
                m_Cam.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
                return;
            }

            // Pillarbox
            if (m_LastAspect > WantedAspectRatio) {
                float inset = 1.0f - WantedAspectRatio/m_LastAspect;
                m_Cam.rect = new Rect(inset/2, 0.0f, 1.0f - inset, 1.0f);
            }

            // Letterbox
            else {
                float inset = 1.0f - m_LastAspect/WantedAspectRatio;
                m_Cam.rect = new Rect(0.0f, inset/2, 1.0f, 1.0f - inset);
            }
        }
    }
}