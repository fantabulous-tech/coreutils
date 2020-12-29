using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

namespace CoreUtils {
    public class RotateWithTarget : MonoBehaviour {
        [SerializeField] private Transform m_Target;
        [SerializeField] private Axis m_TargetAxis = Axis.Y;
        [SerializeField] private Axis m_Axis = Axis.Y;
        [SerializeField] private float m_Scaler = 1;

        private float m_RotationOffset;
        private Quaternion m_StartRotation;
        private Vector3 m_LastTangent;
        private float m_Delta;
        private Vector3 m_MyAxis;

        private void OnEnable() {
            m_MyAxis = GetVectorAxis(m_Axis);
            m_StartRotation = transform.localRotation;
            m_LastTangent = GetTangentAxis(m_TargetAxis, m_Target);
        }

        private void OnDisable() {
            transform.localRotation = m_StartRotation;
        }

        private void FixedUpdate() {
            Vector3 newTangent = GetTangentAxis(m_TargetAxis, m_Target);
            m_Delta = Vector3.SignedAngle(m_LastTangent, newTangent, m_MyAxis);
            m_LastTangent = newTangent;
            m_RotationOffset += m_Delta;
            transform.localRotation = m_StartRotation*Quaternion.AngleAxis(m_RotationOffset*m_Scaler, m_MyAxis);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            Handles.Label(transform.position, $"Rotation: {m_RotationOffset} / Delta: {m_Delta}");
        }
#endif
        private Vector3 GetTangentAxis(Axis axis, Transform t) {
            switch (axis) {
                case Axis.X:
                    return t.up;
                case Axis.Y:
                    return t.forward;
                case Axis.Z:
                    return t.right;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }

        private Vector3 GetVectorAxis(Axis axis) {
            switch (axis) {
                case Axis.X:
                    return Vector3.right;
                case Axis.Y:
                    return Vector3.up;
                case Axis.Z:
                    return Vector3.forward;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }
    }
}