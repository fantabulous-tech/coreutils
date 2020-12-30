using System;
using UnityEngine;

namespace CoreUtils {
    public class AppTracker : Singleton<AppTracker> {
        /// <summary>
        ///     Event raised when the application is quitting.
        /// </summary>
        public static event Action OnQuit;

        /// <summary>
        ///     Returns true if the application is quitting.
        /// </summary>
        public static bool IsQuitting { get; private set; }

        /// <summary>
        ///     Check if we are really playing, works even in editor when some code executes as you enter playmode but before
        ///     Application.IsPlaying is true.
        /// </summary>
        /// <value> true if the application is running or if we are entering playmode </value>
        public static bool IsPlaying {
            get {
                bool isPlaying = Application.isPlaying;
#if UNITY_EDITOR
                isPlaying |= UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode;
#endif
                return isPlaying;
            }
        }

        protected void Awake() {
            Application.quitting += RaiseOnQuit;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += RaiseOnQuit;
#endif
        }

        private void Update() {
            if (!IsPlaying && !IsQuitting) {
                RaiseOnQuit();
            }
        }

#if UNITY_EDITOR
        private static void RaiseOnQuit(UnityEditor.PlayModeStateChange playModeChange) {
            if (playModeChange == UnityEditor.PlayModeStateChange.ExitingPlayMode) {
                RaiseOnQuit();
            }
        }
#endif

        private static void RaiseOnQuit() {
            IsQuitting = true;
            OnQuit?.Invoke();
            OnQuit = null;
        }

        private void OnDestroy() {
            RaiseOnQuit();

            Application.quitting -= RaiseOnQuit;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= RaiseOnQuit;
#endif
        }
    }
}