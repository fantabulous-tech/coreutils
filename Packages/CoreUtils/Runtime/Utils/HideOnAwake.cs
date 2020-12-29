using UnityEngine;

namespace CoreUtils {
    public class HideOnAwake : MonoBehaviour {
        private void Awake() {
            if (gameObject.activeInHierarchy) {
                gameObject.SetActive(false);
            }
        }
    }
}