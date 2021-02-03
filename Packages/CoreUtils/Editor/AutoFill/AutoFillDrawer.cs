using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoreUtils.Editor {
    [CustomPropertyDrawer(typeof(AutoFillAttribute))]
    [CustomPropertyDrawer(typeof(AutoFillFromChildrenAttribute))]
    [CustomPropertyDrawer(typeof(AutoFillFromParentAttribute))]
    [CustomPropertyDrawer(typeof(AutoFillFromSceneAttribute))]
    public class AutoFillDrawer : AttributeDrawer<AutoFillAttribute> {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            AutoFillAttribute autoFill = attribute as AutoFillAttribute;

            if (CanAutoFill(property, autoFill) && NeedsFilling(property)) {
                FillValue(property, autoFill);
            }

            EditorGUI.PropertyField(position, property, label);
        }

        private static bool CanAutoFill(SerializedProperty property, AutoFillAttribute autoFill) {
            if (autoFill == null) {
                return false;
            }

            if (property.isArray) {
                // Unity doesn't call the drawer for the array, it calls it for the elements in the array. :(
                return false;
            }

            if (property.propertyPath.EndsWith("]")) {
                // As above, we use the ] char as a sign that Unity is trying to edit an array element
                return false;
            }

            return property.propertyType == SerializedPropertyType.ObjectReference;
        }

        private static bool NeedsFilling(SerializedProperty property) {
            if (EditorApplication.isPlayingOrWillChangePlaymode) {
                return false;
            }
            if (!property.hasMultipleDifferentValues) {
                if (property.isArray) {
                    return property.arraySize == 0;
                }

                if (property.propertyType == SerializedPropertyType.ObjectReference) {
                    return property.objectReferenceValue == null;
                }
            }

            return false;
        }

        private static void FillValue(SerializedProperty property, AutoFillAttribute autoFill) {
            Object value = null;
            FieldInfo fieldInfo = GetFieldInfoFromProperty(property);
            Type searchType = fieldInfo.FieldType;

            foreach (Object targetObject in property.serializedObject.targetObjects) {
                MonoBehaviour behaviour = targetObject as MonoBehaviour;
                Transform root = UnityUtils.GetTransform(targetObject);

                if (behaviour == null) {
                    Debug.LogWarningFormat(targetObject, "Couldn't find target behaviour on {0}.", targetObject);
                    continue;
                }

                SerializedObject singleObject = new SerializedObject(targetObject);
                SerializedProperty singleProperty = singleObject.FindProperty(property.propertyPath);

                if (singleProperty == null) {
                    Debug.LogWarningFormat(behaviour, "Couldn't find {0} property for {1}", property.propertyPath, behaviour);
                    continue;
                }

                switch (autoFill) {
                    case AutoFillFromChildrenAttribute _:
                        value = FindObjectInChildren(root, searchType);
                        break;
                    case AutoFillFromParentAttribute _:
                        value = FindObjectInParent(root, searchType);
                        break;
                    case AutoFillFromSceneAttribute _:
                        value = Object.FindObjectOfType(searchType);
                        break;
                    default:
                        value = FindObjectInSelf(root, searchType);
                        break;
                }

                if (value == null) {
                    continue;
                }

                SetValue(singleProperty, value);
                singleObject.ApplyModifiedProperties();
            }

            if (value != null) {
                property.serializedObject.SetIsDifferentCacheDirty();
            }
        }

        private static FieldInfo GetFieldInfoFromProperty(SerializedProperty property) {
            Type scriptTypeFromProperty = GetScriptTypeFromProperty(property);
            if (scriptTypeFromProperty == null) {
                return null;
            }
            return GetFieldInfoFromPropertyPath(scriptTypeFromProperty, property.propertyPath);
        }

        private static Dictionary<FieldCacheKey, FieldInfo> s_FieldInfoCache;

        private readonly struct FieldCacheKey : IEquatable<FieldCacheKey> {
            private readonly Type m_Type;
            private readonly string m_Path;

            public FieldCacheKey(Type type, string path) {
                m_Type = type;
                m_Path = path;
            }

            public bool Equals(FieldCacheKey other) {
                return m_Type == other.m_Type && string.Equals(m_Path, other.m_Path);
            }

            public override bool Equals(object obj) {
                return obj is FieldCacheKey other && Equals(other);
            }

            public override int GetHashCode() {
                unchecked {
                    return (m_Type != null ? m_Type.GetHashCode() : 0)*397 ^ (m_Path != null ? m_Path.GetHashCode() : 0);
                }
            }
        }

        private static FieldInfo GetFieldInfoFromPropertyPath(Type host, string path) {
            if (s_FieldInfoCache == null) {
                s_FieldInfoCache = new Dictionary<FieldCacheKey, FieldInfo>();
            }

            FieldInfo fieldInfo = null;
            FieldCacheKey key = new FieldCacheKey(host, path);
            if (!s_FieldInfoCache.TryGetValue(key, out fieldInfo)) {
                Type type = host;
                string[] array = path.Split('.');

                for (int i = 0; i < array.Length; i++) {
                    string text = array[i];
                    if (i < array.Length - 1 && text == "Array") {
                        if (array[i + 1].StartsWith("data[")) {
                            if (IsArrayOrList(type)) {
                                type = GetArrayOrListElementType(type);
                            }

                            i++;
                        } else if (array[i + 1] == "size") {
                            if (IsArrayOrList(type)) {
                                type = GetArrayOrListElementType(type);
                            }

                            i++;
                        }
                    } else {
                        FieldInfo fieldInfo2 = null;
                        Type type2 = type;
                        while (fieldInfo2 == null && type2 != null) {
                            fieldInfo2 = type2.GetField(text, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            type2 = type2.BaseType;
                        }

                        if (fieldInfo2 == null) {
                            fieldInfo = null;
                            break; // Failed to find anything
                        }

                        fieldInfo = fieldInfo2;
                        type = fieldInfo.FieldType;
                    }
                }
            }

            s_FieldInfoCache[key] = fieldInfo;

            return fieldInfo;
        }

        private static bool IsArrayOrList(Type listType) {
            return listType.IsArray || listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(List<>);
        }

        private static Type GetArrayOrListElementType(Type listType) {
            if (listType.IsArray) {
                return listType.GetElementType();
            }
            if (listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(List<>)) {
                return listType.GetGenericArguments()[0];
            }
            return null;
        }

        private static Type GetScriptTypeFromProperty(SerializedProperty property) {
            return property.serializedObject.targetObject.GetType();
        }

        private static void SetValue(SerializedProperty property, Object value) {
            // Don't overwrite the same value as that will cause Prefab variants to become modified
            if (property.objectReferenceValue != value) {
                property.objectReferenceValue = value;
            }
        }

        // We implement the search functions ourselves, because the built in GetComponentInChildren() ones won't search for components on disabled objects
        private static Object FindObjectInSelf(Transform root, Type searchType) {
            return SmarterGetComponent(root, searchType);
        }

        private static Object FindObjectInChildren(Transform root, Type searchType) {
            // We're going to do a breadth first search so that in the case where there are multiple 
            // possible children we favour those closer to the root
            Queue<Transform> queue = new Queue<Transform>();

            queue.Enqueue(root);
            while (queue.Count > 0) {
                Transform t = queue.Dequeue();
                Object obj = SmarterGetComponent(t, searchType);

                if (obj != null) {
                    return obj;
                }

                foreach (Transform child in t) {
                    queue.Enqueue(child);
                }
            }

            return null;
        }

        private static Object FindObjectInParent(Transform root, Type searchType) {
            Transform t = root;
            while (t != null) {
                Object obj = SmarterGetComponent(t, searchType);
                if (obj != null) {
                    return obj;
                }

                t = t.parent;
            }
            return null;
        }

        private static Object SmarterGetComponent(Transform t, Type searchType) {
            if (searchType == typeof(GameObject)) {
                // This is kind of dumb, but we might as well support it, because someone already tried it.... hence why I'm writing this.
                return t.gameObject;
            }
            return t.GetComponent(searchType);
        }
    }
}