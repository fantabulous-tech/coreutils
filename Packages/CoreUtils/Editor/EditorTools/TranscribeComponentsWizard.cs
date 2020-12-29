using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using EditorStyles = UnityEditor.EditorStyles;
using Object = UnityEngine.Object;

namespace CoreUtils.Editor {
    public class TranscribeComponentsWizard : ScriptableWizard {
        [SerializeField] private GameObject m_Source;
        [SerializeField] private GameObject m_Target;
        [SerializeField] private CopyGameObjectTask m_RootTask;
        private Vector2 m_Scroll;
        private static bool s_CreateMissingObjects;

        [MenuItem("Tools/Transcribe Components")]
        private static void CreateTranscribeComponentsWizard() {
            DisplayWizard<TranscribeComponentsWizard>("Transcribe Components", "Transcribe");
        }

        private void OnWizardUpdate() {
            if (m_RootTask == null) {
                m_Source = m_Source ? m_Source : Selection.activeGameObject;
                m_Target = m_Target ? m_Target : Selection.gameObjects.FirstOrDefault(go => go != m_Source);

                if (!AssetDatabase.GetAssetPath(m_Source).IsNullOrEmpty()) {
                    m_Source = null;
                }
                if (!AssetDatabase.GetAssetPath(m_Target).IsNullOrEmpty()) {
                    m_Target = null;
                }

                m_RootTask = new CopyGameObjectTask(m_Source, m_Source, m_Target, m_Target, null);
            }
        }

        private void OnWizardCreate() {
            m_RootTask.Copy();
            m_RootTask.MoveRelativeReferences();
            m_RootTask = new CopyGameObjectTask(m_RootTask.Source, m_RootTask.Source, m_RootTask.Target, m_RootTask.Target, null);
        }

        private void OnGUI() {
            if (m_RootTask == null) {
                OnWizardUpdate();
            }

            s_CreateMissingObjects = EditorGUILayout.Toggle("Create Missing Objects", s_CreateMissingObjects);
            RowLayout layout = new RowLayout();
            GUI.Label(layout.SourceRect, "Source", EditorStyles.boldLabel);
            GUI.Label(layout.TargetRect, "Target", EditorStyles.boldLabel);
            m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
            m_RootTask.OnGUI();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();
            if (GUILayout.Button("Transcribe All")) {
                OnWizardCreate();
            }
        }

        private class CopyGameObjectTask {
            private static Type[] s_ExcludedComponents = {typeof(Transform), typeof(SkinnedMeshRenderer), typeof(MeshRenderer), typeof(MeshFilter)};

            private CopyGameObjectTask m_ParentTask;
            private GameObject m_SourceRoot;
            private GameObject m_TargetRoot;
            private CopyComponentTask[] m_ComponentTasks;
            private CopyGameObjectTask[] m_SubTasks;
            private bool m_ImportantTask;

            public GameObject Source { get; private set; }
            public GameObject Target { get; private set; }

            public CopyGameObjectTask(GameObject sourceRoot, GameObject source, GameObject targetRoot, GameObject target, CopyGameObjectTask parent) {
                m_ParentTask = parent;
                Source = source;
                Target = target;
                m_SourceRoot = sourceRoot;
                m_TargetRoot = targetRoot;

                if (!m_SourceRoot) {
                    return;
                }

                GetComponents();
                GetSubTasks();
            }

            private void GetComponents() {
                Component[] sourceComponents = Source.GetComponents<Component>().Where(NotExcluded).ToArray();
                Component[] targetComponents = Target ? Target.GetComponents<Component>().Where(NotExcluded).ToArray() : new Component[0];
                m_ComponentTasks = new CopyComponentTask[sourceComponents.Length];

                for (int i = 0; i < m_ComponentTasks.Length; i++) {
                    Component sourceComponent = sourceComponents[i];
                    Component targetComponent = sourceComponent ? targetComponents.FirstOrDefault(c => c && c.GetType() == sourceComponent.GetType()) : null;

                    if (targetComponent) {
                        targetComponents[targetComponents.IndexOf(targetComponent)] = null;
                    }

                    m_ComponentTasks[i] = new CopyComponentTask(sourceComponents[i], targetComponent, this);
                }

                if (m_ComponentTasks.Any()) {
                    m_ImportantTask = true;
                }
            }

            private static bool NotExcluded(Component component) {
                return s_ExcludedComponents.All(ec => !ec.IsInstanceOfType(component));
            }

            private void GetSubTasks() {
                GameObject[] sourceChildren = Source.transform.GetChildren().Select(c => c.gameObject).ToArray();
                GameObject[] targetChildren = Target ? Target.transform.GetChildren().Select(c => c.gameObject).ToArray() : new GameObject[0];
                List<CopyGameObjectTask> copyTasks = new List<CopyGameObjectTask>();

                // Name match pass.
                for (int i = 0; i < sourceChildren.Length; i++) {
                    GameObject sourceChild = sourceChildren[i];
                    GameObject targetChild = targetChildren.FirstOrDefault(c => c && c.name.Equals(sourceChild.name, StringComparison.OrdinalIgnoreCase));

                    if (targetChild) {
                        copyTasks.Add(new CopyGameObjectTask(m_SourceRoot, sourceChild, m_TargetRoot, targetChild, this));
                        sourceChildren[i] = null;
                        targetChildren[targetChildren.IndexOf(targetChild)] = null;
                    }
                }

                for (int i = 0; i < sourceChildren.Length; i++) {
                    GameObject sourceChild = sourceChildren[i];

                    if (!sourceChild) {
                        continue;
                    }

                    GameObject targetChild = targetChildren.ElementAtOrDefault(i);

                    if (targetChild) {
                        copyTasks.Add(new CopyGameObjectTask(m_SourceRoot, sourceChild, m_TargetRoot, targetChild, this));
                        targetChildren[i] = null;
                        continue;
                    }

                    copyTasks.Add(new CopyGameObjectTask(m_SourceRoot, sourceChild, m_TargetRoot, null, this));
                }

                m_SubTasks = copyTasks.ToArray();
                m_ImportantTask = m_ImportantTask || m_SubTasks.Any(st => st.m_ImportantTask);
            }

            public void OnGUI(int indentLevel = 0) {
                if (!m_ImportantTask && indentLevel > 0) {
                    return;
                }

                GUILayout.BeginHorizontal();
                RowLayout layout = new RowLayout(indentLevel);

                GameObject newSource = (GameObject) EditorGUI.ObjectField(layout.SourceRect, Source, typeof(GameObject), true);
                
                if (!Target) {
                    if (GUI.Button(layout.DividerRect, "->")) {
                        CopyGameObject();
                        // Copy();
                        // MoveRelativeReferences();
                    }
                }

                GameObject newTarget = (GameObject) EditorGUI.ObjectField(layout.TargetRect, Target, typeof(GameObject), true);
                GUILayout.EndHorizontal();

                m_ComponentTasks.ForEach(ct => ct.OnGUI(indentLevel + 3));
                m_SubTasks.ForEach(st => st.OnGUI(indentLevel + 1));

                if (newSource != Source || newTarget != Target) {
                    if (Source == m_SourceRoot) {
                        m_SourceRoot = newSource;
                    }
                    if (Target == m_TargetRoot) {
                        m_TargetRoot = newTarget;
                    }
                    Source = newSource;
                    Target = newTarget;
                    GetComponents();
                    GetSubTasks();
                }
            }

            public void Copy() {
                if (!m_ImportantTask) {
                    return;
                }

                if (m_ComponentTasks.Any() && !Target) {
                    if (s_CreateMissingObjects) {
                        CopyGameObject();
                        m_ComponentTasks.ForEach(t => t.Copy());
                    } else {
                        Debug.LogWarning($"Can't copy {Source.name} components. No matching target found.", Source);
                    }
                } else {
                    m_ComponentTasks.ForEach(t => t.Copy());
                }

                m_SubTasks.ForEach(t => t.Copy());
            }

            private void CopyGameObject() {
                Target = new GameObject(Source.name);
                Transform newTarget = Target.transform;

                if (!m_ParentTask.Target) {
                    m_ParentTask.CopyGameObject();
                }

                if (!m_ParentTask.Target) {
                    Debug.LogError($"Couldn't create target for {Source.name}", Source);
                    return;
                }

                Transform sourceTransform = Source.transform;
                newTarget.SetParent(m_ParentTask.Target.transform);
                newTarget.localPosition = sourceTransform.localPosition;
                newTarget.localRotation = sourceTransform.localRotation;
                newTarget.localScale = sourceTransform.localScale;
            }

            public void MoveRelativeReferences(string sourceRootPath = null, string targetRootPath = null) {
                if (!m_ImportantTask || !Target) {
                    return;
                }

                sourceRootPath = sourceRootPath ?? m_SourceRoot.FullName(FullName.Parts.FullScenePath);
                targetRootPath = targetRootPath ?? m_TargetRoot.FullName(FullName.Parts.FullScenePath);

                m_ComponentTasks.ForEach(t => t.MoveRelativeReferences(sourceRootPath, targetRootPath));
                m_SubTasks.ForEach(t => t.MoveRelativeReferences(sourceRootPath, targetRootPath));
            }
        }

        private class CopyComponentTask {
            private bool m_Enabled = true;
            private readonly Component m_SourceComponent;
            private Component m_TargetComponent;
            private CopyGameObjectTask m_ParentTask;

            private GameObject Target => m_ParentTask.Target;

            public CopyComponentTask(Component sourceComponent, Component targetComponent, CopyGameObjectTask parent) {
                m_SourceComponent = sourceComponent;
                m_TargetComponent = targetComponent;
                m_ParentTask = parent;
            }

            public void OnGUI(int indentLevel) {
                GUILayout.BeginHorizontal();
                RowLayout layout = new RowLayout(indentLevel, true);
                EditorGUI.ObjectField(layout.SourceRect, m_SourceComponent, typeof(Component), true);
                m_Enabled = GUI.Toggle(layout.ToggleRect, m_Enabled, GUIContent.none);
                if (GUI.Button(layout.DividerRect, "->")) {
                    Copy();
                }
                EditorGUI.ObjectField(layout.TargetRect, m_TargetComponent, typeof(Component), true);
                GUILayout.EndHorizontal();
            }

            public void Copy() {
                if (!m_SourceComponent) {
                    return;
                }

                if (!Target) {
                    Debug.LogError("Expected m_Target, but it's null.");
                    return;
                }

                m_TargetComponent = m_TargetComponent ? m_TargetComponent : Target.AddComponent(m_SourceComponent.GetType());

                if (ComponentUtility.CopyComponent(m_SourceComponent)) {
                    ComponentUtility.PasteComponentValues(m_TargetComponent);
                }
            }

            public void MoveRelativeReferences(string sourceRootPath, string targetRootPath) {
                if (!m_TargetComponent) {
                    return;
                }

                SerializedObject so = new SerializedObject(m_TargetComponent);
                SerializedProperty sp = so.GetIterator();

                while (sp.Next(true)) {
                    MoveRelativeReference(sourceRootPath, targetRootPath, sp);
                }

                so.ApplyModifiedProperties();
            }

            private void MoveRelativeReference(string sourceRootPath, string targetRootPath, SerializedProperty sp) {
                if (sp.propertyType != SerializedPropertyType.ObjectReference) {
                    return;
                }

                Object reference = sp.objectReferenceValue;

                if (!reference || !AssetDatabase.GetAssetPath(reference).IsNullOrEmpty()) {
                    return;
                }

                sourceRootPath += "/";
                targetRootPath += "/";

                string sourcePath = reference.FullName(FullName.Parts.FullScenePath) + "/";

                if (!sourcePath.StartsWith(sourceRootPath, StringComparison.OrdinalIgnoreCase)) {
                    return;
                }

                string targetPath = sourcePath.Replace(sourceRootPath, targetRootPath).TrimEnd('/');
                Debug.Log(m_TargetComponent.GetType().Name + " has a " + reference.GetType().Name + " reference on " + m_TargetComponent.name + ": " + sourcePath + " --> " + targetPath, m_TargetComponent);
                GameObject targetObject = GameObject.Find(targetPath);

                if (!targetObject) {
                    Debug.LogWarning("Couldn't find " + targetPath);
                    return;
                }

                if (reference is GameObject) {
                    sp.objectReferenceValue = targetObject;
                } else {
                    sp.objectReferenceValue = targetObject.GetComponent(reference.GetType());

                    if (!sp.objectReferenceValue) {
                        Debug.LogWarning("Couldn't find a component that matches type " + reference.GetType().Name, reference);
                    }
                }
            }
        }

        private class RowLayout {
            private const float kCheckboxWidth = 15;
            private const float kDividerWidth = 25;
            private const float kIndent = 8;

            public readonly Rect ToggleRect;
            public readonly Rect SourceRect;
            public readonly Rect DividerRect;
            public readonly Rect TargetRect;

            public RowLayout(int indentLevel = 0, bool useCheckbox = false) {
                Rect editorRect = EditorGUILayout.GetControlRect(false);
                float indent = indentLevel*kIndent;
                float columnWidth = (editorRect.width - indent*2 - kDividerWidth)/2;
                float x = indent;

                ToggleRect = new Rect(editorRect) {x = x, width = useCheckbox ? kCheckboxWidth : 0};
                x += ToggleRect.width;
                SourceRect = new Rect(editorRect) {x = x, width = columnWidth - ToggleRect.width};
                x += SourceRect.width;
                DividerRect = new Rect(editorRect) {x = x, width = kDividerWidth};
                x += DividerRect.width + indent;
                TargetRect = new Rect(editorRect) {x = x, width = columnWidth};
            }
        }
    }
}