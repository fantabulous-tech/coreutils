using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CoreUtils.Editor {
    public class ScreenshotTaker : EditorWindow {
        private const string kPrefKey = "ScreenshotTaker.SavePath";

        [SerializeField] private Camera m_ScreenshotCamera;

        private int m_ResWidth = Screen.width*4;
        private int m_ResHeight = Screen.height*4;
        private int m_Scale = 1;
        private string m_Path = "";
        // private RenderTexture m_RenderTexture;
        private bool m_IsTransparent;
        // private float m_LastTime;
        private bool m_TakeHiResShot;
        private string m_LastScreenshot = "";
        private Vector2 m_Scroll;

        [MenuItem("Tools/CoreUtils/Instant High-Res Screenshot Window", false, (int)MenuOrder.Window)]
        public static void ShowWindow() {
            ScreenshotTaker editorWindow = GetWindow<ScreenshotTaker>();
            editorWindow.autoRepaintOnSceneChange = true;
            editorWindow.titleContent = new GUIContent("Screenshot");
            editorWindow.Show();
        }

        private void OnEnable() {
            m_ScreenshotCamera = m_ScreenshotCamera ? m_ScreenshotCamera : FindObjectOfType<Camera>();
            m_Path = EditorPrefs.GetString(kPrefKey, "");
        }

        private void OnGUI() {
            m_Scroll = GUILayout.BeginScrollView(m_Scroll);

            EditorGUILayout.LabelField("Resolution", EditorStyles.boldLabel);
            m_ResWidth = EditorGUILayout.IntField("Width", m_ResWidth);
            m_ResHeight = EditorGUILayout.IntField("Height", m_ResHeight);
            m_IsTransparent = EditorGUILayout.Toggle("Transparent Background", m_IsTransparent);

            m_Scale = EditorGUILayout.IntSlider("Scale", m_Scale, 1, 15);

            // Save Path control.
            float kButtonWidth = 60;
            Rect contentRect = EditorGUI.PrefixLabel(EditorGUILayout.GetControlRect(), new GUIContent("Save Path"));
            Rect textFieldRect = new Rect(contentRect.x, contentRect.y, contentRect.width - kButtonWidth, contentRect.height);
            Rect buttonRect = new Rect(textFieldRect.x + textFieldRect.width, textFieldRect.y, kButtonWidth, textFieldRect.height);
            EditorGUI.TextField(textFieldRect, m_Path);
            if (GUI.Button(buttonRect, "Browse")) {
                GetPath();
            }

            // Camera
            m_ScreenshotCamera = EditorGUILayout.ObjectField("Camera", m_ScreenshotCamera, typeof(Camera), true) as Camera;

            if (m_ScreenshotCamera == null) {
                m_ScreenshotCamera = FindObjectOfType<Camera>();
            }

            EditorGUILayout.LabelField("Default Options", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();

            float halfButtonWidth = EditorGUIUtility.currentViewWidth/2 - 6;

            // Screen Size button
            if (GUILayout.Button("Set To Screen Size", GUILayout.Width(halfButtonWidth))) {
                Vector2 gameView = Handles.GetMainGameViewSize();
                m_ResWidth = (int) gameView.x;
                m_ResHeight = (int) gameView.y;
            }

            // Default Size button
            if (GUILayout.Button("Default Size", GUILayout.Width(halfButtonWidth))) {
                m_ResWidth = 2560;
                m_ResHeight = 1440;
                m_Scale = 1;
            }

            GUILayout.EndHorizontal();

            // Screenshot resolution info.
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Screenshot will be taken at {m_ResWidth*m_Scale} x {m_ResHeight*m_Scale} px", EditorStyles.boldLabel);

            // Take Screenshot button.
            if (GUILayout.Button("Take Screenshot", GUILayout.MinHeight(60))) {
                if (m_Path == "" || !Directory.Exists(m_Path)) {
                    m_Path = EditorUtility.SaveFolderPanel("Path to Save Images", m_Path, Application.dataPath);
                    EditorPrefs.SetString(kPrefKey, m_Path);
                    Debug.Log("Path Set");
                }

                TakeHiResShot();
            }
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            // Open Last Screenshot button.
            GUI.enabled = m_LastScreenshot != "";
            if (GUILayout.Button("Open Last Screenshot", GUILayout.MinHeight(40), GUILayout.Width(halfButtonWidth))) {
                Application.OpenURL("file://" + m_LastScreenshot);
                Debug.Log("Opening File " + m_LastScreenshot);
            }
            GUI.enabled = true;

            // Open Folder button.
            if (GUILayout.Button("Open Folder", GUILayout.MinHeight(40), GUILayout.Width(halfButtonWidth))) {
                Application.OpenURL("file://" + m_Path);
            }

            EditorGUILayout.EndHorizontal();

            if (m_TakeHiResShot) {
                int resWidthN = m_ResWidth*m_Scale;
                int resHeightN = m_ResHeight*m_Scale;
                RenderTexture rt = new RenderTexture(resWidthN, resHeightN, 24);
                m_ScreenshotCamera.targetTexture = rt;

                TextureFormat tFormat = m_IsTransparent ? TextureFormat.ARGB32 : TextureFormat.RGB24;

                Texture2D screenShot = new Texture2D(resWidthN, resHeightN, tFormat, false);
                m_ScreenshotCamera.Render();
                RenderTexture.active = rt;
                screenShot.ReadPixels(new Rect(0, 0, resWidthN, resHeightN), 0, 0);
                m_ScreenshotCamera.targetTexture = null;
                RenderTexture.active = null;
                byte[] bytes = screenShot.EncodeToPNG();
                string filename = ScreenShotName(resWidthN, resHeightN);

                File.WriteAllBytes(filename, bytes);
                Debug.Log($"Took screenshot to: {filename}");
                Application.OpenURL(filename);
                m_TakeHiResShot = false;
            }

            EditorGUILayout.HelpBox("Requires Unity Pro.", MessageType.Info);

            GUILayout.EndScrollView();
        }

        private void GetPath() {
            m_Path = EditorUtility.SaveFolderPanel("Path to Save Images", m_Path, string.IsNullOrEmpty(m_Path) ? Application.dataPath : m_Path);
            EditorPrefs.SetString(kPrefKey, m_Path);
            Debug.Log("Path Set");
        }

        private string ScreenShotName(int width, int height) {
            string strPath = "";
            strPath = $"{m_Path}/screen_{width}x{height}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
            m_LastScreenshot = strPath;
            return strPath;
        }

        private void TakeHiResShot() {
            Debug.Log("Taking Screenshot");
            m_TakeHiResShot = true;
        }
    }
}
