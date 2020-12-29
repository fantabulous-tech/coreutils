using UnityEngine;

namespace CoreUtils {
    public class ShowRandomly : MonoBehaviour {
        [SerializeField, Range(0, 1)] private float m_Chance = 0.5f;

        private void Start() {
            gameObject.SetActive(Random.value < m_Chance);
        }
    }
}