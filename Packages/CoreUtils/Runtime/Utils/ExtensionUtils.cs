using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace CoreUtils {
    public static class ExtensionUtils {
        public static bool Approximately(this Bounds a, Bounds b) {
            return a.min.Approximately(b.min) && a.max.Approximately(b.max);
        }

        public static bool Approximately(this Vector3 a, Vector3 b) {
            return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y) && Mathf.Approximately(a.z, b.z);
        }

        public static bool Approximately(this Vector3 a, Vector3 b, float epsilon) {
            return Mathf.Abs(a.x - b.x) < epsilon && Mathf.Abs(a.y - b.y) < epsilon && Mathf.Abs(a.z - b.z) < epsilon;
        }

        public static bool Approximately(this Quaternion a, Quaternion b) {
            return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y) && Mathf.Approximately(a.z, b.z) && Mathf.Approximately(a.w, b.w);
        }

        public static bool Approximately(this Quaternion a, Quaternion b, float epsilon) {
            return Mathf.Abs(a.x - b.x) < epsilon && Mathf.Abs(a.y - b.y) < epsilon && Mathf.Abs(a.z - b.z) < epsilon && Mathf.Abs(a.w - b.w) < epsilon;
        }

        public static bool Approximately(this float a, float b) {
            return Mathf.Approximately(a, b);
        }

        public static bool Approximately(this float a, float b, float epsilon) {
            return Mathf.Abs(a - b) < epsilon;
        }

        public static bool Approximately(this Transform a, Transform b, Space space = Space.World) {
            if (space == Space.Self) {
                return a.localPosition.Approximately(b.localPosition) && a.localScale.Approximately(b.localScale) &&
                       a.localRotation.Approximately(b.localRotation);
            }

            return a.position.Approximately(b.position) && a.lossyScale.Approximately(b.lossyScale) &&
                   a.rotation.Approximately(b.rotation);
        }

        public static bool Approximately(this DateTime t1, DateTime t2, float epsilon = 1) {
            return Math.Abs((t1 - t2).TotalSeconds) < epsilon;
        }

        public static bool ApproximateAngle(this float a, float b, float epsilon) {
            return Mathf.Abs(Mathf.DeltaAngle(a, b)) < epsilon;
        }

        public static bool ApproximateAngle(this Vector3 a, Vector3 b, float epsilon) {
            return a.x.ApproximateAngle(b.x, epsilon) && a.y.ApproximateAngle(b.y, epsilon) && a.z.ApproximateAngle(b.z, epsilon);
        }

        public static bool Contains(this Bounds b, Bounds other) {
            return b.Contains(other.min) && b.Contains(other.max);
        }

        public static string ToHexStringRGBA(this Color color) {
            Color32 color32 = color;
            return color32.r.ToString("X2") + color32.g.ToString("X2") + color32.b.ToString("X2") + color32.a.ToString("X2");
        }

        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action) {
            if (items == null) {
                return;
            }

            foreach (T obj in items) {
                action(obj);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> items, Action<T, int> action) {
            if (items == null) {
                return;
            }

            int i = 0;
            foreach (T obj in items) {
                action(obj, i);
                i++;
            }
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> list, params T[] except) {
            return Enumerable.Except(list, except);
        }

        public static int IndexOf<T>(this IEnumerable<T> items, Func<T, bool> predicate) {
            if (items == null) {
                throw new ArgumentNullException("items");
            }

            if (predicate == null) {
                throw new ArgumentNullException("predicate");
            }

            int retVal = 0;
            foreach (T item in items) {
                if (predicate(item)) {
                    return retVal;
                }
                retVal++;
            }
            return -1;
        }

        public static int IndexOf<T>(this IEnumerable<T> items, T item) {
            return items.IndexOf(i => EqualityComparer<T>.Default.Equals(item, i));
        }

        public static T GetRandomItem<T>(this IEnumerable<T> items) {
            T[] enumerable = items as T[] ?? items.ToArray();
            return enumerable.Length == 0 ? default : enumerable.ElementAt(Random.Range(0, enumerable.Length));
        }

        public static string NameOrNull(this Object o) {
            return o != null ? o.name : "null";
        }

        public static void IgnoreCollisionsWith(this Collider c, Collider other) {
            if (c != null && c.enabled && c.gameObject.activeInHierarchy
                && other != null && other.enabled && other.gameObject.activeInHierarchy) {
                Physics.IgnoreCollision(c, other);
            }
        }

        public static void EnableCollisionsWith(this Collider c, Collider other) {
            if (c != null && c.enabled && c.gameObject.activeInHierarchy
                && other != null && other.enabled && other.gameObject.activeInHierarchy) {
                Physics.IgnoreCollision(c, other, false);
            }
        }

        public static Ray ForwardRay(this Transform t) {
            if (t == null) {
                throw new ArgumentNullException("t");
            }

            return new Ray(t.position, t.forward);
        }

        public static Vector3 WithX(this Vector3 v, float x) {
            return new Vector3(x, v.y, v.z);
        }

        public static Vector3 WithY(this Vector3 v, float y) {
            return new Vector3(v.x, y, v.z);
        }

        public static Vector3 WithZ(this Vector3 v, float z) {
            return new Vector3(v.x, v.y, z);
        }

        //Swizzle operations to contract Vector3 down to Vector2
        public static Vector2 XY(this Vector3 v) {
            return new Vector2(v.x, v.y);
        }

        public static Vector2 XZ(this Vector3 v) {
            return new Vector2(v.x, v.z);
        }

        public static Vector2 YZ(this Vector3 v) {
            return new Vector2(v.y, v.z);
        }

        public static Vector3 XY(this Vector2 v, float z) {
            return new Vector3(v.x, v.y, z);
        }

        public static Vector3 XZ(this Vector2 v, float y) {
            return new Vector3(v.x, y, v.y);
        }

        public static Vector3 YZ(this Vector2 v, float x) {
            return new Vector3(x, v.x, v.y);
        }

        public static Vector3 ZeroX(this Vector3 v) {
            return new Vector3(0, v.y, v.z);
        }

        public static Vector3 ZeroY(this Vector3 v) {
            return new Vector3(v.x, 0, v.z);
        }

        public static Vector3 ZeroZ(this Vector3 v) {
            return new Vector3(v.x, v.y, 0);
        }

        public static Quaternion SmoothDamp(this Quaternion rot, Quaternion target, ref Quaternion deriv, float time) {
            if (Time.deltaTime < Mathf.Epsilon) {
                return rot;
            }

            // account for double-cover
            float dot = Quaternion.Dot(rot, target);
            float multi = dot > 0f ? 1f : -1f;
            target.x *= multi;
            target.y *= multi;
            target.z *= multi;
            target.w *= multi;
            // smooth damp (nlerp approx)
            Vector4 result = new Vector4(
                Mathf.SmoothDamp(rot.x, target.x, ref deriv.x, time),
                Mathf.SmoothDamp(rot.y, target.y, ref deriv.y, time),
                Mathf.SmoothDamp(rot.z, target.z, ref deriv.z, time),
                Mathf.SmoothDamp(rot.w, target.w, ref deriv.w, time)
            ).normalized;

            // ensure deriv is tangent
            Vector4 derivError = Vector4.Project(new Vector4(deriv.x, deriv.y, deriv.z, deriv.w), result);
            deriv.x -= derivError.x;
            deriv.y -= derivError.y;
            deriv.z -= derivError.z;
            deriv.w -= derivError.w;

            return new Quaternion(result.x, result.y, result.z, result.w);
        }

        public static bool IsNaN(this float f) {
            return float.IsNaN(f);
        }

        public static bool IsNaN(this Vector3 v) {
            return v.x.IsNaN() || v.y.IsNaN() || v.z.IsNaN();
        }

        public static bool IsInfinity(this float f) {
            return float.IsInfinity(f);
        }

        public static bool IsInfinity(this Vector3 v) {
            return v.x.IsInfinity() || v.y.IsInfinity() || v.z.IsInfinity();
        }

        public static float DistanceTo(this Vector3 v, Vector3 other) {
            Vector3 difference = v - other;
            return difference.magnitude;
        }

        public static float SqrDistanceTo(this Vector3 v, Vector3 other) {
            Vector3 difference = v - other;
            return difference.sqrMagnitude;
        }

        public static bool IsEqual<T>(this T[] a, T[] b) {
            if (a == null || b == null) {
                return a == null && b == null;
            }

            if (a.Length != b.Length) {
                return false;
            }

            for (int i = 0; i < a.Length; ++i) {
                if (!a[i].Equals(b[i])) {
                    return false;
                }
            }
            return true;
        }

        public static bool IsEqual<T>(this T[] a, T[] b, Func<T, T, bool> isEqual) {
            if (a == null || b == null) {
                return a == null && b == null;
            }

            if (a.Length != b.Length) {
                return false;
            }

            for (int i = 0; i < a.Length; ++i) {
                if (!isEqual(a[i], b[i])) {
                    return false;
                }
            }

            return true;
        }

        public static T MaxBy<T>(this IEnumerable<T> list, Converter<T, float> converter) {
            T maxItem = default;
            float max = float.MinValue;

            foreach (T item in list) {
                float value = converter(item);

                if (value > max) {
                    max = value;
                    maxItem = item;
                }
            }

            return maxItem;
        }

        public static int ToInt(this Color32 c) {
            return c.r << 0 | c.g << 8 | c.b << 16 | 255 - c.a << 24;
        }

        public static Color32 ToColor32(this int i) {
            Color32 c = new Color32();
            c.r = (byte) (i >> 0 & 0xff);
            c.g = (byte) (i >> 8 & 0xff);
            c.b = (byte) (i >> 16 & 0xff);
            c.a = (byte) (255 - (i >> 24 & 0xff));
            return c;
        }
    }
}