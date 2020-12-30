using UnityEngine;

namespace CoreUtils {
    public static class PoseExtensions {
        public static Pose ToPose(this GameObject g) {
            if (g) {
                return g.transform.ToPose();
            }
            return new Pose(new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1));
        }

        public static Pose ToPose(this Transform t) {
            if (t) {
                return new Pose(t.position, t.rotation);
            }
            return new Pose(new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1));
        }

        public static void SetPose(this Transform t, Pose p) {
            t.SetPositionAndRotation(p.position, p.rotation);
        }
    }
}