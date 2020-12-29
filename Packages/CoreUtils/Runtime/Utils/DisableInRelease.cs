using UnityEngine;

namespace CoreUtils {
    public class DisableInRelease : MonoBehaviour {
        private void Awake() {
            if (!Debug.isDebugBuild) {
                gameObject.SetActive(false);
            }
        }
    }
}