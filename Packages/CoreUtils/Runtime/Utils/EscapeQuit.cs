using JetBrains.Annotations;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CoreUtils {
    public class EscapeQuit : MonoBehaviour {
        private void Update() {
            if (Input.GetKey("escape")) {
                Quit();
            }
        }

        [UsedImplicitly]
        public void Quit() {
#if UNITY_EDITOR
            if (Application.isEditor) {
                EditorApplication.isPlaying = false;
                return;
            }
#endif

            Application.Quit();
        }
    }
}