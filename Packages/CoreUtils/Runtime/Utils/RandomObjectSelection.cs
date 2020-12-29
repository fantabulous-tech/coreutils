using UnityEngine;

namespace CoreUtils {
    public class RandomObjectSelection : MonoBehaviour {
        [SerializeField] private GameObject[] m_Objects;

        private void OnEnable() {
            if (m_Objects == null || m_Objects.Length == 0) {
                return;
            }

            int choice = Random.Range(0, m_Objects.Length);

            for (int i = 0; i < m_Objects.Length; i++) {
                if (i != choice && m_Objects[i]) {
                    m_Objects[i].SetActive(false);
                }
            }

            if (m_Objects[choice]) {
                m_Objects[choice].SetActive(true);
            }
        }
    }
}