using UnityEngine;

namespace CoreUtils {
    public class DisableInBuild : MonoBehaviour {
        private void Start() {
            Debug.LogError("DisableInBuild ran...? This should never happen.", this);
        }

#if UNITY_EDITOR
        [UnityEditor.Callbacks.PostProcessScene]
        public static void OnPostprocessScene() {
            FindObjectsOfType<DisableInBuild>().ForEach(dib => {
                if (dib.enabled) {
                    dib.gameObject.SetActive(false);
                }
            });
        }
#endif
    }
}