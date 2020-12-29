using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CoreUtils.Editor {
    public static class EditorUtils {
        public static void OptionalUnserializedPropertyFieldGUILayout(object target, string propertyName, string label = null) {
            if (ReflectionUtils.HasProperty(target, propertyName)) {
                UnserializedPropertyFieldGUILayout(target, propertyName, label);
            }
        }

        private delegate T OnGUIDelegate<T>(string label, T value, params GUILayoutOption[] options);

        private static void UnserializedPropertyFieldGUILayout(object target, string propertyName, string label = null) {
            label = label ?? propertyName.ToSpacedName();

            Type valueType = ReflectionUtils.GetPropertyType(target, propertyName);

            if (valueType == null) {
                return;
            }

            if (valueType == typeof(bool)) {
                UnserializedPropertyFieldGUILayout<bool>(target, propertyName, label, EditorGUILayout.Toggle);
            } else if (valueType == typeof(int)) {
                UnserializedPropertyFieldGUILayout<int>(target, propertyName, label, EditorGUILayout.IntField);
            } else if (valueType == typeof(float)) {
                UnserializedPropertyFieldGUILayout<float>(target, propertyName, label, EditorGUILayout.FloatField);
            } else if (valueType == typeof(string)) {
                UnserializedPropertyFieldGUILayout<string>(target, propertyName, label, EditorGUILayout.TextField);
            } else if (valueType == typeof(Vector2)) {
                UnserializedPropertyFieldGUILayout<Vector2>(target, propertyName, label, EditorGUILayout.Vector2Field);
            } else if (valueType == typeof(Vector3)) {
                UnserializedPropertyFieldGUILayout<Vector3>(target, propertyName, label, EditorGUILayout.Vector3Field);
            } else if (valueType == typeof(Color)) {
                UnserializedPropertyFieldGUILayout<Color>(target, propertyName, label, EditorGUILayout.ColorField);
            } else if (typeof(Object).IsAssignableFrom(valueType)) {
                UnserializedPropertyFieldGUILayout<Object>(target, propertyName, label, (l, v, p) => EditorGUILayout.ObjectField(l, v, valueType, true, p));
            } else if (valueType.IsEnum) {
                UnserializedPropertyFieldGUILayout<Enum>(target, propertyName, label, EditorGUILayout.EnumPopup);
            } else {
                Debug.LogError("Can't find generic GUI for type " + valueType.Name);
            }
        }

        private static void UnserializedPropertyFieldGUILayout<T>(object target, string propertyName, string label, OnGUIDelegate<T> onGUI) {
            T currentValue = ReflectionUtils.GetPropertyValue<T>(target, propertyName);
            T newValue = onGUI(label, currentValue);
            if (!Equals(newValue, currentValue)) {
                ReflectionUtils.SetPropertyValue(target, propertyName, newValue);
            }
        }

        [MenuItem("GameObject/Sort Siblings", true)]
        public static bool CanSortSiblings() {
            return Selection.transforms.Length > 0;
        }

        [MenuItem("GameObject/Sort Siblings")]
        public static void SortSiblings() {
            if (Selection.transforms.Length == 0) {
                return;
            }
            List<Transform> siblings =
                Selection.transforms.SelectMany(GetSiblings).Distinct().OrderBy(t => t.name).ToList();
            for (int i = 0; i < siblings.Count; i++) {
                siblings[i].SetSiblingIndex(i);
            }
        }

        private static IEnumerable<Transform> GetSiblings(Transform transform) {
            Transform parent = transform.parent;
            return !parent ? GetRootSceneObjects().Select(go => go.transform) : parent.GetChildren();
        }

        [MenuItem("GameObject/Group Selected %g", true)]
        public static bool CanGroupSelected() {
            return Selection.gameObjects.Length > 0;
        }

        [MenuItem("GameObject/Group Selected %g")]
        public static void GroupSelected() {
            if (!Selection.activeTransform) {
                return;
            }
            GameObject go = new GameObject(Selection.activeTransform.name + " Group");
            Undo.RegisterCreatedObjectUndo(go, "Group Selected");
            go.transform.SetParent(Selection.activeTransform.parent, false);
            go.transform.SetSiblingIndex(Selection.activeTransform.GetSiblingIndex());
            go.transform.position = Selection.transforms.Length == 1
                                        ? Selection.transforms[0].position
                                        : UnityUtils.GetCenter(Selection.transforms);
            foreach (Transform transform in Selection.transforms) {
                Undo.SetTransformParent(transform, go.transform, "Group Selected");
            }
            Selection.activeGameObject = go;
        }

        [MenuItem("GameObject/Ungroup Selected %#g", true)]
        public static bool CanUngroupSelected() {
            return Selection.transforms.Any();
        }

        [MenuItem("GameObject/Ungroup Selected %#g")]
        public static void UngroupSelected() {
            if (!Selection.transforms.Any()) {
                return;
            }

            List<Object> deletables = new List<Object>();
            List<Object> selectables = new List<Object>();

            Selection.gameObjects.ForEach(go => {
                if (!go) {
                    return;
                }
                if (go.transform.childCount == 0) {
                    selectables.Add(go);
                    return;
                }
                Transform t = go.transform;
                int index = t.GetSiblingIndex();
                Transform parent = t.parent;
                t.GetChildren().ForEach(c => {
                    if (!c) {
                        return;
                    }
                    Undo.SetTransformParent(c, parent, "Ungroup Selected");
                    c.SetSiblingIndex(index);
                    index++;
                    selectables.Add(c.gameObject);
                });
                deletables.Add(go);
            });

            deletables.Distinct().ForEach(Undo.DestroyObjectImmediate);
            Selection.objects = selectables.ToArray();
        }

        public delegate void ApplyHandler(GameObject instance);

        public static event ApplyHandler OnApply;
        public static event ApplyHandler OnApplied;

        [MenuItem("GameObject/Apply Selected %#a", true)]
        public static bool CanApplySelected() {
            return Selection.gameObjects.Any(IsInstance);
        }

        [MenuItem("GameObject/Apply Selected %#a")]
        public static void ApplySelected() {
            List<string> appliedList = new List<string>();
            foreach (
                GameObject instance in
                Selection.gameObjects.Select<GameObject, Object>(PrefabUtility.GetOutermostPrefabInstanceRoot)
                         .Distinct()
                         .OfType<GameObject>()) {
                Transform root = instance.transform.root;

                if (!instance.transform.IsChildOf(root.transform)) {
                    Debug.LogWarning(string.Format("Can't apply to {0}. Not a child of the root '{1}'", instance.name, root.name), instance);
                    continue;
                }

                GameObject prefab = GetPrefab(instance);

                if (OnApply != null) {
                    OnApply(instance);
                }

                appliedList.Add(AssetDatabase.GetAssetPath(prefab));
                if (OnApplied != null) {
                    OnApplied(instance);
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Applied changes to " + appliedList.Count + " prefabs.\n" + appliedList.AggregateToString("\n"));
        }

        [MenuItem("GameObject/Really Break Prefab Instance", true)]
        public static bool CanReallyBreakPrefab() {
            return Selection.gameObjects.Select(PrefabUtility.GetCorrespondingObjectFromSource).Any(p => p);
        }

        [MenuItem("GameObject/Really Break Prefab Instance")]
        public static void ReallyBreakPrefab() {
            Selection.gameObjects.ForEach(BreakPrefab);
        }

        private static void BreakPrefab(GameObject original) {
            if (!PrefabUtility.GetCorrespondingObjectFromSource(original)) {
                return;
            }
            GameObject root = PrefabUtility.GetOutermostPrefabInstanceRoot(original);
            Transform parent = root.transform.parent;
            int index = root.transform.GetSiblingIndex();
            Undo.SetTransformParent(root.transform, null, "Really Break Prefab");
            root.transform.parent = null;
            GameObject go = Object.Instantiate(root, parent, true); // clears prefab link
            Undo.RegisterCreatedObjectUndo(go, "Really Break Prefab");
            go.name = root.name;
            go.SetActive(root.activeSelf);
            go.transform.SetSiblingIndex(index);
            Object[] selectedObjects = Selection.gameObjects.Cast<Object>().ToArray();
            int selectedIndex = selectedObjects.IndexOf(original);
            if (selectedIndex >= 0) {
                selectedObjects[selectedIndex] = go;
                Selection.objects = selectedObjects;
            }
            Undo.DestroyObjectImmediate(root);
            Debug.Log(string.Format("We broke {0} for realsies.", go.name), go);
        }

        [MenuItem("GameObject/Zero Selected %#z", true)]
        public static bool CanZeroSelected() {
            return Selection.gameObjects.Any();
        }

        [MenuItem("GameObject/Zero Selected %#z")]
        public static void ZeroSelected() {
            Selection.transforms.ForEach(Zero);
        }

        [MenuItem("GameObject/Zero Selected To Children Center", true)]
        public static bool CanZeroSelectedToCenter() {
            return Selection.gameObjects.Any();
        }

        [MenuItem("GameObject/Zero Selected To Children Center")]
        public static void ZeroSelectedToCenter() {
            Selection.transforms.ForEach(ZeroToCenter);
        }

        [MenuItem("GameObject/Revert Selected", true)]
        public static bool CanRevertSelected() {
            return Selection.gameObjects.Any(IsInstance);
        }

        [MenuItem("GameObject/Revert Selected")]
        public static void RevertSelected() {
            foreach (GameObject instance in Selection.gameObjects) {
                Undo.RegisterCompleteObjectUndo(instance, "Revert Selected");
                Vector3 position = instance.transform.localPosition;
                Quaternion rotation = instance.transform.localRotation;
                Vector3 scale = instance.transform.localScale;
                PrefabUtility.RevertPrefabInstance(instance, InteractionMode.UserAction);
                instance.transform.localPosition = position;
                instance.transform.localRotation = rotation;
                instance.transform.localScale = scale;
            }
            AssetDatabase.SaveAssets();
        }

        [MenuItem("GameObject/Select Source Prefab", true)]
        public static bool CanSelectSourcePrefab() {
            return Selection.gameObjects.Any(IsInstance);
        }

        [MenuItem("GameObject/Select Source Prefab")]
        public static bool SelectSourcePrefab() {
            Object prefab = PrefabUtility.GetCorrespondingObjectFromSource(Selection.activeGameObject);
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
            Debug.Log(prefab.name, prefab);
            return prefab;
        }

        public static GameObject GetPrefab(Object instance) {
            GameObject go = UnityUtils.GetGameObject(instance);
            if (!go) {
                return null;
            }
            GameObject root = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
            if (!root) {
                return null;
            }
            return PrefabUtility.GetCorrespondingObjectFromSource(root);
        }

        public static bool IsInstance(GameObject go) {
            GameObject prefab = GetPrefab(go);
            return prefab && prefab != go;
        }

        public static IEnumerable<GameObject> GetRootSceneObjects() {
            List<GameObject> objects = new List<GameObject>();

            for (int i = 0; i < SceneManager.sceneCount; i++) {
                Scene scene = SceneManager.GetSceneAt(i);
                objects.AddRange(scene.GetRootGameObjects());
            }
            return objects;
        }

        public static IEnumerable<Transform> GetRootTransforms() {
            return GetRootSceneObjects().Select(go => go.transform);
        }

        [MenuItem("File/Save Project Shortcut %&#s")]
        public static void SaveProject() {
            AssetDatabase.SaveAssets();
            Debug.Log("Project saved.");
        }

        public static IEnumerable<AnimatorState> GetStateNames(Animator animator) {
            AnimatorController controller = animator ? animator.runtimeAnimatorController as AnimatorController : null;
            return controller == null ? null : controller.layers.SelectMany(l => l.stateMachine.states).Select(s => s.state);
        }

        public static IEnumerable<AnimatorState> GetStateNamesForLayer(Animator animator, int layerId) {
            AnimatorController controller = animator ? animator.runtimeAnimatorController as AnimatorController : null;
            if (layerId < 0 || controller != null && layerId >= controller.layers.Length) {
                return null;
            }
            return controller == null ? null : controller.layers[layerId].stateMachine.states.Select(s => s.state);
        }

        public static Color BackgroundColor => EditorGUIUtility.isProSkin ? new Color32(56, 56, 56, 255) : new Color32(194, 194, 194, 255);

        public static bool ButtonToggle(bool condition, string label, GUIStyle style, params GUILayoutOption[] options) {
            bool toggleVal = condition;
            toggleVal = GUILayout.Toggle(toggleVal, label, style, options);
            return toggleVal != condition;
        }

        /// <summary>
        ///     Resets the transform's local position, rotation, and scale without moving the its children relative to the global
        ///     space.
        /// </summary>
        /// <param name="transform">Transform to zero out.</param>
        public static void Zero(Transform transform) {
            Undo.RecordObject(transform, "Zero Selected");
            IEnumerable<Transform> children = OrphanChildren(transform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            ReadoptChildren(children, transform);
        }

        public static void ZeroToCenter(Transform transform) {
            Undo.RecordObject(transform, "Zero Selected");
            Vector3 center = UnityUtils.GetCenter(transform);
            IEnumerable<Transform> children = OrphanChildren(transform);
            transform.position = center;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            ReadoptChildren(children, transform);
        }

        private static IEnumerable<Transform> OrphanChildren(Transform transform) {
            Transform[] children = new Transform[transform.childCount];

            for (int i = transform.childCount - 1; i >= 0; i--) {
                Transform child = transform.GetChild(i);
                Undo.RecordObject(child, "Zero Selected");
                children[i] = child;
                Undo.SetTransformParent(child, null, "Zero Selected");
            }

            return children;
        }

        private static void ReadoptChildren(IEnumerable<Transform> children, Transform transform) {
            foreach (Transform child in children) {
                Undo.SetTransformParent(child, transform, "Zero Selected");
            }
        }

        private static BindingFlags s_SerializedPropertyFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

        public static Type GetPropertyType(SerializedProperty property) {
            Type parentType = property.serializedObject.targetObject.GetType();
            FieldInfo fi = parentType.GetFieldViaPath(property.propertyPath);
            Debug.Assert(fi != null, $"No field info found named '{property.propertyPath}' on {parentType.FullName}");
            return fi.FieldType;
        }

        private static FieldInfo GetFieldViaPath(this Type type, string path) {
            Type parentType = type;
            FieldInfo fi = null;

            string[] perDot = path.Split('.');
            foreach (string fieldName in perDot) {
                fi = parentType.GetFieldInfoRecursive(fieldName);

                if (fi == null) {
                    return null;
                }

                parentType = fi.FieldType;
            }

            return fi;
        }

        private static FieldInfo GetFieldInfoRecursive(this Type type, string fieldName) {
            if (type == null) {
                return null;
            }

            FieldInfo fi = type.GetField(fieldName, s_SerializedPropertyFlags);

            // Try getting base type fields since serialized private properties can't retrieved from inherited types.
            if (fi == null) {
                fi = GetFieldInfoRecursive(type.BaseType, fieldName);
            }

            return fi;
        }

        public static string GetGuidFromAsset(Object obj) {
            return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
        }

        public static T GetAssetFromGuid<T>(string guid) where T : Object {
            return (T) GetAssetFromGuid(guid, typeof(T));
        }

        public static Object GetAssetFromGuid(string guid, Type type) {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath(path, type);
        }

        public static bool GetEditorPrefBool(ref int backingInt, string prefKey, int defaultValue = 0) {
            if (backingInt < 0) {
                backingInt = EditorPrefs.GetInt(prefKey, defaultValue);
            }

            return backingInt > 0;
        }

        public static void SetEditorPrefBool(ref int backingInt, bool value, string prefKey) {
            if (backingInt > 0 == value) {
                return;
            }

            backingInt = value ? 1 : 0;
            EditorPrefs.SetInt(prefKey, backingInt);
        }
    }
}