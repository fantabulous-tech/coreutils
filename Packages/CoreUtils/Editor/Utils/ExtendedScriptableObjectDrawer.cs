using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoreUtils.Editor {

    /// <summary>
    ///     Extends how ScriptableObject object references are displayed in the inspector
    ///     Shows you all values under the object reference
    ///     Also provides a button to create a new ScriptableObject if property is null.
    /// </summary>
    [CustomPropertyDrawer(typeof(ScriptableObject), true)]
    public class ExtendedScriptableObjectDrawer : PropertyDrawer {

        #region EditorPref

        private const string kExpandedScriptableObjectsEnabledPref = "ExpandedScriptableObjectsEnabled";
        private static bool s_Enabled = true;

        [InitializeOnLoadMethod]
        private static void OnLoad() {
            s_Enabled = EditorPrefs.GetBool(kExpandedScriptableObjectsEnabledPref);
        }

        [SettingsProvider, UsedImplicitly]
        private static SettingsProvider GetSettingsProvider() {
            return new SettingsProvider("Preferences/Scriptable Object Expander", SettingsScope.User) {
                guiHandler = searchContext => {
                    bool enabled = EditorGUILayout.Toggle("Enabled", s_Enabled);

                    if (enabled == s_Enabled) {
                        return;
                    }

                    s_Enabled = enabled;
                    EditorPrefs.SetBool(kExpandedScriptableObjectsEnabledPref, s_Enabled);
                }
            };
        }

        #endregion

        #region GUIStyles

        private static GUIStyle s_Toolbar;
        private static GUIStyle s_BoldToolbarButton;
        private static GUIStyle s_ToolbarLabel;
        private static GUIStyle s_SubEditorBox;
        private static GUIStyle s_ErrorLabel;

        private static GUIStyle Toolbar {
            get {
                if (s_Toolbar != null) {
                    return s_Toolbar;
                }

                s_Toolbar = new GUIStyle(EditorStyles.toolbar) {padding = new RectOffset(5, 0, 0, 0)};
                return s_Toolbar;
            }
        }

        private static GUIStyle BoldToolbarButton {
            get {
                if (s_BoldToolbarButton != null) {
                    return s_BoldToolbarButton;
                }

                s_BoldToolbarButton = new GUIStyle(EditorStyles.toolbarButton) {alignment = TextAnchor.MiddleCenter, fontSize = 11, fontStyle = FontStyle.Bold};
                return s_BoldToolbarButton;
            }
        }

        private static GUIStyle ToolbarLabel {
            get {
                if (s_ToolbarLabel != null) {
                    return s_ToolbarLabel;
                }

                s_ToolbarLabel = new GUIStyle(EditorStyles.toolbar) {alignment = TextAnchor.MiddleLeft, fontSize = 11, fontStyle = FontStyle.Bold};
                return s_ToolbarLabel;
            }
        }

        private static GUIStyle SubEditorBox {
            get {
                if (s_SubEditorBox != null) {
                    return s_SubEditorBox;
                }

                s_SubEditorBox = new GUIStyle(GUI.skin.box) {margin = new RectOffset(0, 0, 0, 0)};
                return s_SubEditorBox;
            }
        }

        private static GUIStyle ErrorLabel => s_ErrorLabel ?? (s_ErrorLabel = new GUIStyle(GUI.skin.label) {normal = {textColor = Color.red}});

        #endregion

        private readonly Dictionary<Object, UnityEditor.Editor> m_EditorLookup = new Dictionary<Object, UnityEditor.Editor>();

        private readonly float m_WarningInfoHeight = EditorGUIUtility.singleLineHeight*4;
        private string m_Path;
        private string m_FileName;
        private string m_ErrorInfo;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (m_ErrorInfo == null) {
                m_ErrorInfo = CheckForValidCustomEditor(property);
            }

            if (!property.isExpanded || m_ErrorInfo == "") {
                return base.GetPropertyHeight(property, label);
            }

            return base.GetPropertyHeight(property, label) + m_WarningInfoHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (!s_Enabled) {
                EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property, label);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            if (property.objectReferenceValue != null) {
                OnValidObjectGUI(position, property, label);
            } else {
                OnNullReferenceGUI(position, property, label);
            }

            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }

        private void OnValidObjectGUI(Rect position, SerializedProperty property, GUIContent label) {
            bool hasLabel = label != GUIContent.none;
            float labelWidth = hasLabel ? EditorGUIUtility.labelWidth : 0;

            if (hasLabel) {
                property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, labelWidth, EditorGUIUtility.singleLineHeight), property.isExpanded, label, true);
            }

            EditorGUI.PropertyField(new Rect(position.x + labelWidth - 15*EditorGUI.indentLevel, position.y, position.width - labelWidth + 15*EditorGUI.indentLevel, EditorGUIUtility.singleLineHeight), property, GUIContent.none, true);

            if (GUI.changed) {
                property.serializedObject.ApplyModifiedProperties();
            }

            if (hasLabel && property.isExpanded) {
                using (new EditorGUI.IndentLevelScope()) {
                    m_ErrorInfo = m_ErrorInfo ?? CheckForValidCustomEditor(property);

                    if (m_ErrorInfo == "") {
                        OnScriptableObjectEditorGUI(property);
                    } else {
                        OnCustomEditorWarningGUI(property, position);
                    }
                }
            } else {
                m_ErrorInfo = null;
            }
        }

        private void OnNullReferenceGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.ObjectField(new Rect(position.x, position.y, position.width - 60, EditorGUIUtility.singleLineHeight), property, label);
            if (GUI.Button(new Rect(position.x + position.width - 58, position.y, 58, EditorGUIUtility.singleLineHeight), "Create")) {
                string selectedAssetPath = "Assets";
                MonoBehaviour component = property.serializedObject.targetObject as MonoBehaviour;
                if (component != null) {
                    MonoScript ms = MonoScript.FromMonoBehaviour(component);
                    selectedAssetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(ms));
                }

                if (property.serializedObject.targetObject is ScriptableObject) {
                    selectedAssetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(property.serializedObject.targetObject));
                }

                Type type = fieldInfo.FieldType;
                if (type.IsArray) {
                    type = type.GetElementType();
                } else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) {
                    type = type.GetGenericArguments()[0];
                }

                property.objectReferenceValue = CreateAssetWithSavePrompt(type, selectedAssetPath, property.name.ReplaceRegex("^m_", ""));
            }
        }

        private void OnCustomEditorWarningGUI(SerializedProperty property, Rect position) {
            bool wasEnabled = GUI.enabled;
            GUI.enabled = true;
            Rect labelRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight*2, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.SelectableLabel(labelRect, m_ErrorInfo, ErrorLabel);
            GUIContent buttonName = new GUIContent("Create Custom Editor");
            Vector2 size = GUI.skin.button.CalcSize(buttonName);
            float posX = position.x + position.width/2 - size.x/2;
            if (GUI.Button(new Rect(posX, labelRect.y + EditorGUIUtility.singleLineHeight, size.x, size.y), buttonName)) {
                SaveEditor(property);
            }
            GUI.enabled = wasEnabled;
        }

        private static void SaveEditor(SerializedProperty property) {
            Type componentType = property.serializedObject.targetObject.GetType();
            MonoScript script = GetScript(property.serializedObject.targetObject);
            string scriptPath = AssetDatabase.GetAssetPath(script);
            string directory = Path.GetDirectoryName(scriptPath) + "/Editor/";
            UnityUtils.CreateFoldersFor(directory);

            string path = EditorUtility.SaveFilePanel("Save " + componentType.Name + " Custom Editor", directory, componentType.Name + "Editor.cs", "cs");

            if (path.IsNullOrEmpty()) {
                return;
            }

            path = path.ReplaceRegex("^.*/Assets/", "Assets/");

            string text = string.Format(@"using UnityEditor;

namespace {0} {{
	// NOTE: This CustomEditor allows the ScriptableObjects referenced in {1} to be expandable in the Inspector.
	[CustomEditor(typeof({1}))]
	public class {1}Editor : UnityEditor.Editor {{ }}
}}", componentType.Namespace.IsNullOrEmpty() ? "CoreUtils" : componentType.Namespace, componentType.Name);

            UnityUtils.CreateFoldersFor(path);
            File.WriteAllText(path, text);
            AssetDatabase.ImportAsset(path);
        }

        private static MonoScript GetScript(Object obj) {
            MonoBehaviour mb = obj as MonoBehaviour;

            if (mb) {
                return MonoScript.FromMonoBehaviour(mb);
            }

            ScriptableObject so = obj as ScriptableObject;

            if (so) {
                return MonoScript.FromScriptableObject(so);
            }

            Debug.LogError("Cannot get script from " + obj + ". It is not a MonoBehaviour or a ScriptableObject.", obj);

            return null;
        }

        private void OnScriptableObjectEditorGUI(SerializedProperty property) {
            UnityEditor.Editor editor = GetEditor(property.objectReferenceValue);

            if (m_Path == null) {
                m_Path = AssetDatabase.GetAssetPath(property.objectReferenceValue);
                m_FileName = Path.GetFileNameWithoutExtension(m_Path);
            }

            Asset assetFile;
            bool canEdit = CanEdit(m_Path, out assetFile);

            GUILayout.BeginHorizontal(Toolbar);

            GUI.enabled = canEdit;
            GUILayout.Label(m_FileName + " Inspector", ToolbarLabel, GUILayout.ExpandWidth(true));
            GUI.enabled = true;

            if (Provider.enabled && !canEdit && GUILayout.Button("Check Out", BoldToolbarButton, GUILayout.ExpandWidth(false))) {
                Provider.Checkout(assetFile, CheckoutMode.Both);
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginVertical(SubEditorBox);
            using (new EditorGUI.DisabledGroupScope(Provider.enabled && !canEdit)) {
                editor.OnInspectorGUI();
            }

            GUILayout.EndVertical();
        }

        private static string CheckForValidCustomEditor(SerializedProperty property) {
            Type parentType = property.serializedObject.targetObject.GetType();
            UnityEditor.Editor parentEditor = UnityEditor.Editor.CreateEditor(property.serializedObject.targetObject);
            Type parentEditorType = parentEditor.GetType();
            Type assignedEditorType = GetAttributeValue(parentEditorType, (CustomEditor a) => GetField<Type>(a, "m_InspectedType"));
            bool validCustomEditor = assignedEditorType == parentType;
            return validCustomEditor ? "" : GetErrorInfo(property);
        }

        private static string GetErrorInfo(SerializedProperty property) {
            string parentType = property.serializedObject.targetObject.GetType().Name;
            string errorInfo = string.Format("NOTE: To show this ScriptableObject editor, a custom editor is needed for {0}. Use this button to make it.", parentType);
            return errorInfo;
        }

        private static bool CanEdit(string assetPath, out Asset asset) {
            asset = null;

            if (!Provider.enabled || string.IsNullOrEmpty(assetPath)) {
                return true;
            }

            asset = Provider.GetAssetByPath(assetPath);

            if (asset != null) {
                return Provider.IsOpenForEdit(asset);
            }

            Task task = Provider.Status(assetPath, false);
            task.Wait();
            asset = task.assetList.Count <= 0 ? null : task.assetList[0];
            return asset != null && Provider.IsOpenForEdit(asset);
        }

        private UnityEditor.Editor GetEditor(Object obj) {
            UnityEditor.Editor editor;

            if (!m_EditorLookup.TryGetValue(obj, out editor)) {
                editor = m_EditorLookup[obj] = UnityEditor.Editor.CreateEditor(obj);
            }

            return editor;
        }

        private static TValue GetAttributeValue<TAttribute, TValue>(Type type, Func<TAttribute, TValue> valueSelector) where TAttribute : Attribute {
            TAttribute att = type.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;
            return att != null ? valueSelector(att) : default;
        }

        private static T GetField<T>(object obj, string propertyName) {
            Type t = obj.GetType();

            foreach (FieldInfo field in t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)) {
                if (field.Name != propertyName) {
                    continue;
                }

                if (field.FieldType == typeof(T)) {
                    return (T) field.GetValue(obj);
                }

                Debug.LogWarning("Property found, but it isn't of type " + typeof(T).Name);
                return default;
            }

            Debug.LogWarning("Couldn't find property " + propertyName + " on " + obj);
            return default;
        }

        // Creates a new ScriptableObject via the default Save File panel
        private static ScriptableObject CreateAssetWithSavePrompt(Type type, string directory, string name) {
            directory = EditorUtility.SaveFilePanelInProject("Save ScriptableObject", name + ".asset", "asset", "Enter a file name for the ScriptableObject.", directory);
            if (directory == "") {
                return null;
            }

            ScriptableObject asset = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(asset, directory);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(directory, ImportAssetOptions.ForceUpdate);
            EditorGUIUtility.PingObject(asset);
            return asset;
        }
    }
}