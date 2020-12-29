using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoreUtils.Editor {
    public class ReorderableListGUI<T> {
        private const int kMoveTabWidth = 15;
        private const int kSpacing = 5;

        private readonly string m_Name;
        private readonly ReorderableList m_List;
        private readonly List<Column> m_AssignedColumns = new List<Column>();
        private readonly List<Column> m_Columns = new List<Column>();
        private readonly string m_Error;
        private readonly bool m_IsReference;

        private Rect m_Rect;

        private bool NeedsTitle => !m_IsReference && m_AssignedColumns.Count > 1;

        public ReorderableListGUI(SerializedObject parentObj, string listName, string displayName = null) {
            SerializedProperty listProperty = parentObj.FindProperty(listName);

            if (listProperty == null) {
                m_Error = string.Format("'{0}' not found{1}.", listName, parentObj.targetObject ? " on " + parentObj.targetObject.name : "");
                Debug.LogError(m_Error, parentObj.targetObject);
                m_Name = !displayName.IsNullOrEmpty() ? displayName : listName;
                return;
            }

            m_IsReference = typeof(T) == typeof(Object) || typeof(T).IsSubclassOf(typeof(Object));
            m_Name = !displayName.IsNullOrEmpty() ? displayName : listProperty.displayName;
            m_List = new ReorderableList(parentObj, parentObj.FindProperty(listName), true, true, true, true);
            m_List.drawHeaderCallback += OnHeaderGUI;
            m_List.drawElementCallback += OnItemGUI;

            UpdateDisplayColumns();
        }

        private void OnHeaderGUI(Rect rect) {
            m_Rect = rect;
            m_Rect.width -= kMoveTabWidth;

            if (NeedsTitle) {
                EditorGUI.LabelField(new Rect(m_Rect.x, m_Rect.y - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing, m_Rect.width, EditorGUIUtility.singleLineHeight), m_Name, EditorStyles.boldLabel);
            }

            m_Columns.ForEach(c => c.OnHeaderGUI(m_Rect));

            m_Rect.x += kMoveTabWidth;
        }

        private void OnItemGUI(Rect rect, int index, bool isActive, bool isFocused) {
            m_Rect.y = rect.y + 2;
            m_Rect.height = Mathf.Max(m_List.elementHeight - EditorGUIUtility.standardVerticalSpacing, EditorGUIUtility.singleLineHeight);

            bool wasWrapped = EditorStyles.textField.wordWrap;
            EditorStyles.textField.wordWrap = rect.height > EditorGUIUtility.singleLineHeight;

            SerializedProperty itemProperty = m_List.serializedProperty.GetArrayElementAtIndex(index);
            SerializedObject so = null;
            if (m_IsReference && itemProperty.objectReferenceValue) {
                so = new SerializedObject(itemProperty.objectReferenceValue);
                so.Update();
            }

            m_Columns.ForEach(c => c.OnGUI(itemProperty, m_Rect, m_IsReference, so));

            if (so != null) {
                so.ApplyModifiedProperties();
            }

            EditorStyles.textField.wordWrap = wasWrapped;
        }

        private void UpdateColumnWidths() {
            int flexSpace = (int) EditorGUILayout.GetControlRect(false).width - kMoveTabWidth - 10;

            if (flexSpace < 0) {
                return;
            }

            int flexColumnCount = m_Columns.Count;

            m_Columns.ForEach(c => c.UpdateFlexSpace(ref flexSpace, ref flexColumnCount));

            int flexWidth = flexColumnCount == 0 ? 0 : (flexSpace - kSpacing*(m_Columns.Count - 1))/flexColumnCount;
            int offset = 0;
            m_Columns.ForEach(c => c.UpdateDisplayWidth(flexWidth, ref offset));
            if (flexSpace < 0) {
                Debug.LogWarning(string.Format("Not enough space for set columns. ({0})", flexSpace), m_List.serializedProperty.serializedObject.targetObject);
            }
        }

        private void UpdateDisplayColumns() {
            m_Columns.Clear();

            if (m_IsReference) {
                m_Columns.Add(new Column());
            }

            if (m_AssignedColumns.Count == 0 && m_Columns.Count == 0) {
                m_Columns.Add(new Column());
            }

            m_Columns.AddRange(m_AssignedColumns);
            m_Columns.ForEach(c => c.UpdateHeaderName(m_List.serializedProperty, m_IsReference));
        }

        /// <summary>
        ///     Adds a column to the Reorderable List.
        /// </summary>
        /// <param name="propertyPath">The path to the property on the serialzied list item.</param>
        /// <param name="width">
        ///     The column's width in pixels. If &lt;= 0, the column is auto-sized based on the other
        ///     flexible columns. default = flexible.
        /// </param>
        public void AddColumn(string propertyPath, int width = -1) {
            AddColumn(propertyPath, null, width);
        }

        /// <summary>
        ///     Adds a column to the Reorderable List.
        /// </summary>
        /// <param name="propertyPath">The path to the property on the serialzied list item.</param>
        /// <param name="title">The title of the column. default = propety's display name.</param>
        /// <param name="width">
        ///     The column's width in pixels. If &lt;= 0, the column is auto-sized based on the other
        ///     flexible columns. default = flexible.
        /// </param>
        public void AddColumn(string propertyPath, string title, int width = -1) {
            m_AssignedColumns.Add(new Column(propertyPath, title, width));
            UpdateDisplayColumns();
        }

        /// <summary>
        ///     Adds a button column to the Reorderable List that enables actions durion gameplay.
        /// </summary>
        /// <param name="functionPath">The path to the action function on the serialzied list item.</param>
        /// <param name="columnName">The name to use for the button.</param>
        /// <param name="buttonName">Name of the column's button.</param>
        /// <param name="width">
        ///     The column's width in pixels. If &lt;= 0, the column is auto-sized based on the other
        ///     flexible columns. default = 80.
        /// </param>
        public void AddButtonColumn(string functionPath, string columnName, string buttonName, int width = 80) {
            m_AssignedColumns.Add(new ButtonColumn(functionPath, columnName, buttonName, width));
            UpdateDisplayColumns();
        }

        /// <summary>
        ///     Sets the height of each element in pixels.
        /// </summary>
        /// <param name="height"></param>
        public void SetElementHeight(float height) {
            m_List.elementHeight = height;
        }

        /// <summary>
        ///     Create the GUI for this reorderable list. Use this in the 'OnInspectorGUI' function or other editor-based OnGUI
        ///     calls.
        /// </summary>
        public void OnGUI() {
            if (m_Error != null) {
                EditorGUILayout.LabelField(m_Name, m_Error, CustomEditorStyle.ErrorLabel);
                return;
            }

            if (NeedsTitle) {
                EditorGUILayout.Space();
            }

            m_List.serializedProperty.serializedObject.Update();

            UpdateColumnWidths();
            m_List.DoLayoutList();

            if (GUI.changed) {
                m_List.serializedProperty.serializedObject.ApplyModifiedProperties();
                UpdateDisplayColumns();
            }
        }

        private class ButtonColumn : Column {
            private readonly string m_ButtonName;

            public ButtonColumn(string actionPath, string columnName, string buttonName, int width) : base(actionPath, columnName, width) {
                m_ButtonName = buttonName;
            }

            public override void OnGUI(SerializedProperty itemProperty, Rect rowRect, bool isReference, SerializedObject so) {
                bool wasEnabled = GUI.enabled;

                GUI.enabled = Application.isPlaying;

                if (GUI.Button(GetRect(rowRect), m_ButtonName)) {
                    RunAction(itemProperty);
                }

                GUI.enabled = wasEnabled;
            }

            private void RunAction(SerializedProperty itemProperty) {
                object obj = itemProperty.GetTargetObject();

                if (obj == null) {
                    Debug.LogError("No object found.");
                } else {
                    Type type = obj.GetType();
                    MethodInfo action = type.GetMethod(m_PropertyPath, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    if (action != null) {
                        action.Invoke(obj, null);
                    } else {
                        Debug.LogWarning(string.Format("Could not find '{0}' function on {1}", m_PropertyPath, type.Name), itemProperty.serializedObject.targetObject);
                    }
                }
            }
        }

        private class Column {
            protected readonly string m_PropertyPath;

            private readonly int m_AssignedWidth = -1;
            private readonly string m_AssignedName;
            private readonly string m_PropertyName;

            private string m_Name;
            private int m_Width;
            private int m_Offset;

            public Column() { }

            public Column(string propertyPath, string name, int width) {
                m_AssignedName = name;
                m_AssignedWidth = width;
                m_PropertyPath = propertyPath;
                m_PropertyName = propertyPath == null ? null : propertyPath.Split('.').GetLast();
            }

            public void OnHeaderGUI(Rect rect) {
                int previousIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                EditorGUI.LabelField(GetRect(rect, kMoveTabWidth), m_Name);
                EditorGUI.indentLevel = previousIndent;
            }

            public void UpdateFlexSpace(ref int flexSpace, ref int flexColumnCount) {
                if (m_AssignedWidth > 0) {
                    flexSpace -= m_AssignedWidth;
                    flexColumnCount--;
                }
            }

            public void UpdateDisplayWidth(int flexWidth, ref int offset) {
                m_Offset = offset;
                m_Width = m_AssignedWidth > 0 ? m_AssignedWidth : flexWidth;
                offset += m_Width + kSpacing;
            }

            protected Rect GetRect(Rect rect, int offset = 0) {
                return new Rect(rect.x + m_Offset + offset, rect.y, m_Width, rect.height);
            }

            public void UpdateHeaderName(SerializedProperty listProperty, bool isReference) {
                if (!m_AssignedName.IsNullOrEmpty()) {
                    m_Name = m_AssignedName;
                    return;
                }

                if (m_PropertyName.IsNullOrEmpty()) {
                    m_Name = listProperty.displayName;
                    return;
                }

                if (!isReference) {
                    SerializedProperty property = listProperty.FindPropertyRelative(m_PropertyPath);
                    if (property != null) {
                        m_Name = property.displayName;
                        return;
                    }
                }

                m_Name = m_PropertyName.ToSpacedName();
            }

            public virtual void OnGUI(SerializedProperty itemProperty, Rect rowRect, bool isReference, SerializedObject so) {
                int previousIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                Object context = so == null ? itemProperty.serializedObject.targetObject : so.targetObject;

                if (m_PropertyPath.IsNullOrEmpty()) {
                    // If we have no property path, then we are representing the item itself.
                    OnItemGUI(itemProperty, rowRect, isReference, so);
                } else if (!isReference) {
                    // Since we are not a reference object, we can get the relative property.
                    OnPropertyGUI(itemProperty.FindPropertyRelative(m_PropertyPath), rowRect, context);
                } else if (so != null) {
                    // Since we ARE a reference object AND we have a non-null serialzied object,
                    // we can find the property for the serialzied object.
                    OnPropertyGUI(so.FindProperty(m_PropertyPath), rowRect, context);
                } else {
                    // Otherwise, we have a null reference object, so we can't find the property on it. Let's just put an empty label here.
                    EditorGUI.LabelField(GetRect(rowRect), "--");
                }

                EditorGUI.indentLevel = previousIndent;
            }

            private void OnItemGUI(SerializedProperty itemProperty, Rect rowRect, bool isReference, SerializedObject so) {
                SerializedProperty displayProperty;

                // Not a property, so we are showing the reference or object itself.
                if (isReference) {
                    displayProperty = itemProperty;
                } else if (so != null) {
                    displayProperty = so.FindProperty("name") ?? so.FindProperty("Name") ?? so.FindProperty("m_Name");
                    EditorGUI.LabelField(GetRect(rowRect), displayProperty == null ? itemProperty.displayName : displayProperty.displayName);
                    return;
                } else {
                    EditorGUI.LabelField(GetRect(rowRect), itemProperty.displayName);
                    return;
                }

                OnPropertyGUI(displayProperty, rowRect, itemProperty.serializedObject.targetObject);
            }

            private void OnPropertyGUI(SerializedProperty property, Rect rowRect, Object context) {
                Rect rect = GetRect(rowRect);

                if (property == null) {
                    string error = string.Format("'{0}' not found", m_PropertyPath);
                    Debug.LogError(error, context);
                    EditorGUI.LabelField(rect, error, CustomEditorStyle.ErrorLabel);
                    return;
                }

                EditorGUI.PropertyField(rect, property, GUIContent.none, false);
            }
        }
    }
}