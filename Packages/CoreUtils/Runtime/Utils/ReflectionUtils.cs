using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CoreUtils {
    public static class ReflectionUtils {
        private const BindingFlags kAllBindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly Dictionary<string, PropertyInfo> s_PropertyCache = new Dictionary<string, PropertyInfo>();

        public static Type GetPropertyType(object target, string propertyName) {
            PropertyInfo info = target.GetType().FindProperty(propertyName);
            return info != null ? info.PropertyType : null;
        }

        public static bool HasProperty(object target, string propertyName) {
            return target.GetType().FindProperty(propertyName) != null;
        }

        private static PropertyInfo FindProperty(this Type type, string propertyName) {
            string propertyPath = string.Format("{0}.{1}", type.FullName, propertyName);
            PropertyInfo info;
            try {
                info = s_PropertyCache[propertyPath];
            }
            catch {
                info = s_PropertyCache[propertyPath] = type.GetProperty(propertyName, kAllBindings);

                if (info == null) {
                    Debug.LogWarning("Property not found: " + propertyPath);
                }
            }

            return info;
        }

        public static T GetPropertyValue<T>(object obj, string propertyName) {
            return (T) obj.GetType().FindProperty(propertyName).GetValue(obj, null);
        }

        public static void SetPropertyValue(object obj, string propertyName, object value) {
            obj.GetType().FindProperty(propertyName).SetValue(obj, value, null);
        }
    }
}