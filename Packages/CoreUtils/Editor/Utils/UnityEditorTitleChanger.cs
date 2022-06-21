#if UNITY_EDITOR_WIN

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace CoreUtils.Editor {
    public static class UnityEditorTitleChanger {
        private static string s_OverrideWindowText;
        private static string s_DefaultWindowName;
        private static bool s_IsOverridden;
        private static IntPtr s_EditorWindow;

        private static int s_OverrideUnityWindowName = -1;
        private static int s_IncludeVersionControlInfo = -1;

        private const string kOverrideUnityWindowName = "CoreUtils.OverrideUnityWindowName";
        private const string kIncludeVersionControlInfo = "CoreUtils.IncludeVersionControlInfo";

        private static bool OverrideUnityWindowName {
            get => EditorUtils.GetEditorPrefBool(ref s_OverrideUnityWindowName, kOverrideUnityWindowName);
            set => EditorUtils.SetEditorPrefBool(ref s_OverrideUnityWindowName, value, kOverrideUnityWindowName);
        }

        private static bool IncludeVersionControlInfo {
            get => EditorUtils.GetEditorPrefBool(ref s_IncludeVersionControlInfo, kIncludeVersionControlInfo);
            set => EditorUtils.SetEditorPrefBool(ref s_IncludeVersionControlInfo, value, kIncludeVersionControlInfo);
        }

        private static bool Initialized => !s_DefaultWindowName.IsNullOrEmpty();

        private delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int cch);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(HandleRef handle, out int processId);

        [DllImport("user32.dll", EntryPoint = "SetWindowText")]
        private static extern bool SetWindowText(IntPtr hWnd, string lpString);

        [InitializeOnLoadMethod]
        private static void LoadTitleChanger() {
            CoreUtilsSettings.Register("Unity Title Changer", OnGUI);
            TryInit();
            EditorApplication.update += Update;
            Update();
        }

        private static void TryInit() {

            // If we already have the default window name, no reason to init.
            if (Initialized) {
                return;
            }

            int editorProcessId = Process.GetCurrentProcess().Id;

            // Create a filter to find the matching editor window.
            bool Filter(IntPtr hWnd, int lParam) {
                GetWindowThreadProcessId(new HandleRef(null, hWnd), out int processId);

                if (processId != editorProcessId) {
                    return true;
                }

                string title = GetWindowTitle(hWnd);

                if (title.IsNullOrEmpty() || !title.Contains("Unity")) {
                    return true;
                }

                s_EditorWindow = hWnd;
                s_DefaultWindowName = title;
                return true;
            }

            // Run through all desktop windows to find and cache the Unity editor window.
            EnumDesktopWindows(IntPtr.Zero, Filter, IntPtr.Zero);

            // If we found the proper window, set the override window text.
            if (Initialized) {
                s_OverrideWindowText = GetOverrideWindowTitle();
            }
        }

        private static void Update() {
            TryInit();

            // If we haven't found the proper editor window yet, don't set any window names.
            if (!Initialized) {
                return;
            }

            // Update the Unity window name based on preferences.
            if (OverrideUnityWindowName) {
                SetWindowText(s_EditorWindow, s_OverrideWindowText);
                s_IsOverridden = true;
            } else if (s_IsOverridden) {
                SetWindowText(s_EditorWindow, s_DefaultWindowName);
                s_IsOverridden = false;
            }
        }

        private static string GetWindowTitle(IntPtr hWnd) {
            StringBuilder titleBuilder = new StringBuilder(255);
            GetWindowText(hWnd, titleBuilder, titleBuilder.Capacity + 1);
            return titleBuilder.ToString();
        }

        private static string GetOverrideWindowTitle() {
            if (!IncludeVersionControlInfo) {
                return $"{GetProjectFolderName()} - Unity {Application.unityVersion}";
            }

            string vcInfo = GetVersionControlInfo();
            return $"{GetProjectFolderName()} - Unity {Application.unityVersion} - {vcInfo}";
        }

        private static string GetVersionControlInfo() {
            Plugin activePlugin = Provider.GetActivePlugin();

            if (activePlugin == null) {
                return "[no version control]";
            }

            // Try to find the workspace info for Perforce.
            foreach (ConfigField field in activePlugin.configFields) {
                if (field.name.Contains("workspace", StringComparison.OrdinalIgnoreCase)) {
                    return EditorUserSettings.GetConfigValue(field.name);
                }
            }

            // Not sure what other info to include. Just return the version control name.
            return activePlugin.name;
        }

        private static string GetProjectFolderName() {
            string path = Application.dataPath;
            path = path.Replace("/Assets", "");

            string[] pieces = path.Split('/');
            int nonClientIndex = pieces.IndexOf(pieces.LastOrDefault(p => !p.Contains("client", StringComparison.OrdinalIgnoreCase)));

            if (nonClientIndex >= 0) {
                path = pieces.Skip(nonClientIndex).AggregateToString("/");
            }

            return path;
        }

        private static void OnGUI() {
            bool overrideUnityWindowName = EditorGUILayout.Toggle(new GUIContent("Override Window Name", "This changed Unity's window name to include more of the path info."), OverrideUnityWindowName);

            if (OverrideUnityWindowName != overrideUnityWindowName) {
                OverrideUnityWindowName = overrideUnityWindowName;
                Update();
            }

            GUI.enabled = OverrideUnityWindowName;

            bool includeVersionControlInfo = EditorGUILayout.Toggle(new GUIContent("Include VC Info", "Include version control info in the Unity window title."), IncludeVersionControlInfo);

            if (includeVersionControlInfo != IncludeVersionControlInfo) {
                IncludeVersionControlInfo = includeVersionControlInfo;
                s_OverrideWindowText = GetOverrideWindowTitle();
                Update();
            }

            GUI.enabled = true;

            // if (GUILayout.Button("Test")) {
            //     TryInit();
            // }
        }
    }
}

#endif
