using UnityEngine;

namespace CoreUtils {
    public static class TransformExtensions {
        public static Quaternion TransformRotation(this Transform transform, Quaternion rotation) {
            Quaternion world = transform.rotation*rotation;
            NormalizeQuaternion(ref world);
            return world;
        }

        public static Quaternion InverseTransformRotation(this Transform transform, Quaternion rotation) {
            Quaternion local = Quaternion.Inverse(transform.rotation)*rotation;
            NormalizeQuaternion(ref local);
            return local;
        }

        private static void NormalizeQuaternion(ref Quaternion q) {
            float sum = 0;
            for (int i = 0; i < 4; ++i) {
                sum += q[i]*q[i];
            }

            float magnitudeInverse = 1/Mathf.Sqrt(sum);
            for (int i = 0; i < 4; ++i) {
                q[i] *= magnitudeInverse;
            }
        }

        public static void SetGlobalScale(this Transform transform, Vector3 globalScale) {
            transform.localScale = Vector3.one;
            transform.localScale = new Vector3(globalScale.x/transform.lossyScale.x, globalScale.y/transform.lossyScale.y, globalScale.z/transform.lossyScale.z);
        }
    }
}