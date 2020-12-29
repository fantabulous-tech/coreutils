using System;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CoreUtils {
    /// <summary>
    ///     Flags passed to the FullName() extension method to indicate which part of the name to print
    ///     Lives in its own static class so that the name doesn't have to include the horrible "FullNameExtensions"
    /// </summary>
    public static class FullName {
        [Flags]
        public enum Parts {
            TypeName = 1 << 0,
            FullTypeName = 1 << 1 | TypeName,
            Name = 1 << 2,
            FullScenePath = 1 << 3 | Name,
            AssetPath = 1 << 4,
            SiblingIndex = 1 << 5,

            Default = FullTypeName | FullScenePath,
            UniqueName = Name | SiblingIndex,
            UniquePath = FullScenePath | SiblingIndex,
            FullSceneOrAssetPath = FullScenePath | AssetPath,
            All = FullSceneOrAssetPath | FullTypeName
        }
    }

    /// <summary>
    ///     Extension method for UnityEngine.Object that allow getting a more descriptive name/path/type for debug logging
    /// </summary>
    public static class FullNameExtensions {
        public static string FullName(this Object o, FullName.Parts parts = CoreUtils.FullName.Parts.Default, int maxLength = 0) {
            if (o == null) {
                return "null";
            }

            bool contentBeforeType = false;
            bool contentBeforeName = false;

            StringBuilder builder = new StringBuilder();

#if UNITY_EDITOR
            if (FlagSet(parts, CoreUtils.FullName.Parts.AssetPath)) {
                string assetPath = AssetDatabase.GetAssetPath(o);
                if (!string.IsNullOrEmpty(assetPath)) {
                    builder.Append(assetPath);
                    contentBeforeType = true;
                    contentBeforeName = true;
                } else {
                    builder.Append(SceneManager.GetActiveScene().name);
                    contentBeforeType = true;
                    contentBeforeName = true;
                }
            }
#endif

            Transform t = GetTransform(o);

            if (FlagSet(parts, CoreUtils.FullName.Parts.Name)) {
                if (contentBeforeName) {
                    builder.Append(" ");
                }
                if (FlagSet(parts, CoreUtils.FullName.Parts.FullScenePath)) {
                    if (t != null) {
                        BuildScenePath(t, builder, parts);
                        contentBeforeType = true;
                    } else {
                        builder.Append(o.name);
                        contentBeforeType = true;
                    }
                } else {
                    builder.Append(o.name);
                    contentBeforeType = true;
                }
            }

            if (FlagSet(parts, CoreUtils.FullName.Parts.SiblingIndex) && t) {
                builder.AppendFormat("[{0}]", t.GetSiblingIndex());
            }

            if (FlagSet(parts, CoreUtils.FullName.Parts.TypeName)) {
                if (contentBeforeType) {
                    builder.Append(":");
                }

                Type type = o.GetType();
                if (FlagSet(parts, CoreUtils.FullName.Parts.FullTypeName)) {
                    builder.Append(type.FullName);
                } else {
                    builder.Append(type.Name);
                }
            }

            string fullName = builder.ToString();

            if (maxLength > 0 && fullName.Length > maxLength) {
                const string kInsert = "...";
                int prefixLength = (maxLength - kInsert.Length)/3;
                int suffixLength = maxLength - (kInsert.Length + prefixLength);
                string prefix = fullName.Substring(0, prefixLength);
                string suffix = fullName.Substring(fullName.Length - suffixLength, suffixLength);
                return prefix + kInsert + suffix;
            }
            return fullName;
        }

        private static bool FlagSet(FullName.Parts parts, FullName.Parts mask) {
            return (parts & mask) != 0;
        }

        private static void BuildScenePath(Transform t, StringBuilder builder, FullName.Parts parts) {
            if (t.parent != null) {
                BuildScenePath(t.parent, builder, parts);
                if (FlagSet(parts, CoreUtils.FullName.Parts.SiblingIndex)) {
                    builder.AppendFormat("[{0}]", t.parent.GetSiblingIndex());
                }
                builder.Append("/");
            }
            builder.Append(t.name);
        }

        private static Transform GetTransform(Object o) {
            Component c = o as Component;
            if (c != null) {
                return c.transform;
            }

            GameObject go = o as GameObject;
            if (go != null) {
                return go.transform;
            }

            return null;
        }
    }
}