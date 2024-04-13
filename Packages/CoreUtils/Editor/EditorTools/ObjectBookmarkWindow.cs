using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace CoreUtils.Editor {
    /// <summary>
    ///     This window stores references to scene object and project objects in the player preferences as strings.
    ///     If the object exists then a button is presented that allows you to reselect it.
    /// </summary>
    public class ObjectBookmarkWindow : EditorWindow {
        private Vector2 m_ScrollPosition;
        private List<ObjectBookmark> m_Bookmarks;
        private ReorderableList m_BookmarksGUI;
        private ObjectBookmark m_DeleteMe;

        private const string kPrefsKeyBookmarkItemCount = "ObjectBookmarkWindow_ItemCount";
        private const string kPrefsKeyBookMarkIsSceneObjectFormat = "ObjectBookmarkWindow_Item{0}_IsSceneObject";
        private const string kPrefsKeyBookMarkObjectGUIDFormat = "ObjectBookmarkWindow_Item{0}_GUID";
        private const string kPrefsKeyBookMarkObjectScenePathFormat = "ObjectBookmarkWindow_Item{0}_ScenePath";

        private const float kPadding = 2;
        private const float kButtonWidth = 22;
        private const float kDisplayTextBuffer = 5;

        private static string m_ProjectKeyPrefix;

        private static string ProjectKeyPrefix => UnityUtils.GetOrSet(ref m_ProjectKeyPrefix, () => Path.GetFileName(Directory.GetCurrentDirectory()) + "_");

        private static readonly string s_KeyItemCount = ProjectKeyPrefix + kPrefsKeyBookmarkItemCount;
        private static readonly string s_KeyIsSceneObjectFormat = ProjectKeyPrefix + kPrefsKeyBookMarkIsSceneObjectFormat;
        private static readonly string s_KeyObjectGUIDFormat = ProjectKeyPrefix + kPrefsKeyBookMarkObjectGUIDFormat;
        private static readonly string s_KeyObjectScenePathFormat = ProjectKeyPrefix + kPrefsKeyBookMarkObjectScenePathFormat;

        [MenuItem("Tools/CoreUtils/Object Bookmarks Window", false, (int)MenuOrder.Window)]
        public static void OpenWindow() {
            ObjectBookmarkWindow window = (ObjectBookmarkWindow)GetWindow(typeof(ObjectBookmarkWindow), false, "Bookmarks");
            window.Show();
        }

        private void Awake() {
            LoadProfile();
        }

        public void OnGUI() {
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

            if (m_Bookmarks == null) {
                LoadProfile();
            }

            m_BookmarksGUI.DoLayoutList();

            Object obj = EditorGUILayout.ObjectField(new GUIContent("Add New Bookmark", "Drag/drop new scene and asset objects here to bookmark them."), null, typeof(Object), true);
            if (obj != null) {
                AddBookmark(obj);
            }

            EditorGUILayout.EndScrollView();
        }

        private void AddBookmark(Object obj) {
            ObjectBookmark newBookmark = GetBookmark(obj);

            if (newBookmark != null) {
                m_Bookmarks.Add(newBookmark);
                SaveProfile();
            }
        }

        private ObjectBookmark GetBookmark(Object obj) {
            if (EditorUtility.IsPersistent(obj)) {
                // Asset database object
                return new AssetBookmark(obj);
            }

            GameObject go = obj as GameObject;
            if (go) {
                // Scene object
                return new SceneBookmark(go);
            }

            Debug.LogWarning("ObjectBookmarks: Can't create a bookmark for a scene object that isn't a GameObject");
            return null;
        }

        private ObjectBookmark GetSavedBookmark(int index) {
            string isSceneObjectProfileKey = string.Format(s_KeyIsSceneObjectFormat, index);
            string objectGUIDProfileKey = string.Format(s_KeyObjectGUIDFormat, index);
            string objectScenePathProfileKey = string.Format(s_KeyObjectScenePathFormat, index);

            if (!EditorPrefs.HasKey(isSceneObjectProfileKey)) {
                Debug.LogError($"ObjectBookmarks: Couldn't find type of object for object #{index}", this);
                return null;
            }

            bool isSceneObject = EditorPrefs.GetInt(isSceneObjectProfileKey) == 1;

            if (isSceneObject) {
                string scenePath = EditorPrefs.GetString(objectScenePathProfileKey);
                return new SceneBookmark(scenePath);
            }

            string guid = EditorPrefs.GetString(objectGUIDProfileKey);
            return !guid.IsNullOrEmpty() ? new AssetBookmark(guid) : new ObjectBookmark();
        }

        private void LoadProfile() {
            // Debug.Log("ObjectBookmarks: Loading.", this);
            m_ScrollPosition = new Vector2(0.0f, 0.0f);
            m_Bookmarks = new List<ObjectBookmark>();
            m_BookmarksGUI = new ReorderableList(m_Bookmarks, typeof(ObjectBookmark), true, false, false, false) {headerHeight = 0, footerHeight = 0};
            m_BookmarksGUI.drawElementCallback += OnDrawElement;
            m_BookmarksGUI.onChangedCallback += OnListChanged;
            m_BookmarksGUI.drawFooterCallback += OnDrawFooter;

            int itemCount = 0;

            if (EditorPrefs.HasKey(s_KeyItemCount)) {
                itemCount = EditorPrefs.GetInt(s_KeyItemCount);
            }

            for (int i = 0; i < itemCount; ++i) {
                ObjectBookmark newBookmark = GetSavedBookmark(i);

                if (newBookmark != null) {
                    m_Bookmarks.Add(newBookmark);
                }
            }
        }

        private void SaveProfile() {
            // Debug.Log("ObjectBookmarks: Saving.", this);
            EditorPrefs.SetInt(s_KeyItemCount, m_Bookmarks.Count);

            for (int i = 0; i < m_Bookmarks.Count; ++i) {
                string isSceneObjectProfileKey = string.Format(s_KeyIsSceneObjectFormat, i);
                string objectGUIDProfileKey = string.Format(s_KeyObjectGUIDFormat, i);
                string objectScenePathProfileKey = string.Format(s_KeyObjectScenePathFormat, i);

                EditorPrefs.SetInt(isSceneObjectProfileKey, m_Bookmarks[i] is SceneBookmark ? 1 : 0);
                EditorPrefs.SetString(objectGUIDProfileKey, m_Bookmarks[i].ItemRef);
                EditorPrefs.SetString(objectScenePathProfileKey, m_Bookmarks[i].ItemRef);
            }
        }

        private void OnListChanged(ReorderableList list) {
            SaveProfile();
        }

        private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused) {
            ObjectBookmark bookmark = m_Bookmarks[index];
            if (bookmark.OnGUI(rect)) {
                m_DeleteMe = bookmark;
            }

            if (bookmark.NewObject) {
                m_Bookmarks[index] = GetBookmark(bookmark.NewObject);
                SaveProfile();
            }
        }

        private void OnDrawFooter(Rect rect) {
            if (m_DeleteMe != null) {
                m_Bookmarks.Remove(m_DeleteMe);
                m_DeleteMe = null;
                SaveProfile();
            }
        }

        private class ObjectBookmark {
            protected string m_DisplayString;
            private float m_LastButtonWidth;

            protected virtual string FullName => null;
            public virtual string ItemRef => null;
            public Object NewObject { get; private set; }

            protected void UpdateDisplayString(float buttonWidth) {
                if (m_LastButtonWidth.Approximately(buttonWidth)) {
                    return;
                }

                m_LastButtonWidth = buttonWidth;
                m_DisplayString = FullName;

                string trimmedString = m_DisplayString;
                float currentWidth = GUI.skin.label.CalcSize(new GUIContent(m_DisplayString)).x;
                int startIndex = 0;
                while (currentWidth > buttonWidth - kDisplayTextBuffer && startIndex < m_DisplayString.Length) {
                    ++startIndex;
                    trimmedString = "..." + m_DisplayString.Substring(startIndex, m_DisplayString.Length - startIndex);
                    currentWidth = GUI.skin.label.CalcSize(new GUIContent(trimmedString)).x;
                }

                m_DisplayString = trimmedString;
            }

            public virtual bool OnGUI(Rect rect) {
                Rect selectButtonRect = new Rect(rect.x, rect.y, rect.width - kButtonWidth - kPadding, rect.height);
                Rect deleteButtonRect = new Rect(selectButtonRect.x + selectButtonRect.width + kPadding, rect.y, kButtonWidth, rect.height);
                UpdateDisplayString(selectButtonRect.width);

                NewObject = EditorGUI.ObjectField(selectButtonRect, null, typeof(Object), true);

                if (GUI.Button(deleteButtonRect, new GUIContent("X", "Remove bookmark."))) {
                    return true;
                }

                return false;
            }
        }

        private class AssetBookmark : ObjectBookmark {
            private readonly string m_GUID;

            protected override string FullName {
                get {
                    string assetPath = AssetDatabase.GUIDToAssetPath(m_GUID);
                    return assetPath.IsNullOrEmpty() ? $"Missing: {m_GUID.Substring(0, 7)}..." : assetPath;
                }
            }
            public override string ItemRef => m_GUID;

            public AssetBookmark(Object obj) {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                m_GUID = AssetDatabase.AssetPathToGUID(assetPath);
            }

            public AssetBookmark(string guid) {
                m_GUID = guid;
            }

            public override bool OnGUI(Rect rect) {
                string assetPath = AssetDatabase.GUIDToAssetPath(m_GUID);
                GUI.enabled = !assetPath.IsNullOrEmpty();

                Rect selectButtonRect = new Rect(rect.x, rect.y, rect.width - kButtonWidth*2 - kPadding*2, rect.height);
                Rect openAssetButtonRect = new Rect(rect.x + selectButtonRect.width + kPadding, rect.y, kButtonWidth, rect.height);
                Rect deleteButtonRect = new Rect(openAssetButtonRect.x + kButtonWidth + kPadding, rect.y, kButtonWidth, rect.height);
                UpdateDisplayString(selectButtonRect.width);

                if (GUI.Button(selectButtonRect, new GUIContent(m_DisplayString, "Select asset in Project Window."))) {
                    Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(assetPath);
                    EditorGUIUtility.PingObject(Selection.activeObject);
                }

                if (GUI.Button(openAssetButtonRect, new GUIContent("↗", "Open asset."))) {
                    if (Directory.Exists(assetPath)) {
                        Process.Start(assetPath.Replace('/', '\\'));
                    } else {
                        AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object)));
                    }
                }

                GUI.enabled = true;
                return GUI.Button(deleteButtonRect, new GUIContent("X", "Remove bookmark."));
            }
        }

        private class SceneBookmark : ObjectBookmark {
            private readonly string m_ScenePath;

            protected override string FullName => m_ScenePath;
            public override string ItemRef => m_ScenePath;

            public SceneBookmark(GameObject go) {
                m_ScenePath = GetGameObjectPath(go.transform);
            }

            public SceneBookmark(string scenePath) {
                m_ScenePath = scenePath;
            }

            public override bool OnGUI(Rect rect) {
                Rect selectButtonRect = new Rect(rect.x, rect.y, rect.width - kButtonWidth - kPadding, rect.height);
                Rect deleteButtonRect = new Rect(selectButtonRect.x + selectButtonRect.width + kPadding, rect.y, kButtonWidth, rect.height);
                UpdateDisplayString(selectButtonRect.width);

                GameObject displayObject = GetSceneGameObject(m_ScenePath);
                GUI.enabled = displayObject != null;

                if (GUI.Button(selectButtonRect, new GUIContent(m_DisplayString, "Select asset in Project Window."))) {
                    Selection.activeObject = displayObject;
                }

                GUI.enabled = true;

                if (GUI.Button(deleteButtonRect, new GUIContent("X", "Remove bookmark."))) {
                    return true;
                }

                return false;
            }

            private static GameObject GetSceneGameObject(string path) {
                for (int i = 0; i < SceneManager.sceneCount; i++) {
                    Scene scene = SceneManager.GetSceneAt(i);
                    foreach (GameObject rootGameObject in scene.GetRootGameObjects()) {
                        Transform subObject = GetObjectFromPath(rootGameObject.transform, path);

                        if (subObject != null) {
                            return subObject.gameObject;
                        }
                    }
                }

                return null;
            }

            private static Transform GetObjectFromPath(Transform root, string path) {
                if (path == root.name) {
                    return root;
                }

                string prefix = root.name + "/";

                if (!path.StartsWith(prefix)) {
                    return null;
                }

                string subPath = path[prefix.Length..];
                for (int i = 0; i < root.childCount; i++) {
                    Transform subObject = GetObjectFromPath(root.GetChild(i), subPath);

                    if (subObject) {
                        return subObject;
                    }
                }

                return null;
            }

            private static string GetGameObjectPath(Transform transform) {
                string path = transform.name;

                while (transform.parent != null) {
                    transform = transform.parent;
                    path = transform.name + "/" + path;
                }

                return path;
            }
        }
    }
}
