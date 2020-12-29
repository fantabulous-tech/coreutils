using System;
using System.Collections;
using System.Reflection;
using UnityEditor;

namespace CoreUtils.Editor {
    public static class PropertyUtils {
        public static object GetTargetObject(this SerializedProperty prop) {
            string path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            string[] elements = path.Split('.');
            for (int i = 0; i < elements.Length; i++) {
                string element = elements[i];
                if (element.Contains("[")) {
                    string elementName = element.Substring(0, element.IndexOf("["));
                    int index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue(obj, elementName, index);
                } else {
                    obj = GetValue(obj, element);
                }
            }
            return obj;
        }

        private static object GetValue(object source, string name) {
            if (source == null) {
                return null;
            }
            Type type = source.GetType();
            FieldInfo f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f == null) {
                PropertyInfo p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                return p == null ? null : p.GetValue(source, null);
            }
            return f.GetValue(source);
        }

        private static object GetValue(object source, string name, int index) {
            IEnumerable enumerable = (IEnumerable) GetValue(source, name);
            IEnumerator enm = enumerable.GetEnumerator();
            while (index-- >= 0) {
                enm.MoveNext();
            }
            return enm.Current;
        }
    }
}