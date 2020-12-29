using System;
using System.Linq;
using UnityEngine;

namespace CoreUtils {
    public class FollowTargets : MonoBehaviour {
        private enum RotationType {
            YOnly,
            Full,
            None
        }

        private enum MoveType {
            Full,
            StickToFloor,
            None
        }

        [SerializeField] private RotationType m_RotationType;
        [SerializeField] private MoveType m_MoveType;
        [SerializeField] private Vector3 m_PositionOffset = new Vector3(0f, 0f, 0f);
        [SerializeField] private Vector3 m_RotationOffset = Vector3.zero;
        [SerializeField] private float m_RotationSwim;
        [SerializeField] private Transform[] m_Targets;

        public Transform Target { get; private set; }

        private void LateUpdate() {
            if (Target == null || !Target.gameObject.activeInHierarchy) {
                Target = m_Targets.FirstOrDefault(t => t != null && t.gameObject.activeInHierarchy);
            }
            if (Target == null) {
                return;
            }

            UpdateRotation();
            UpdatePosition();
        }

        private void UpdatePosition() {
            if (m_MoveType == MoveType.None) {
                return;
            }

            Transform t = transform;

            switch (m_MoveType) {
                case MoveType.Full:
                    t.position = Target.position + Quaternion.AngleAxis(t.rotation.eulerAngles.y, Vector3.up)*m_PositionOffset;
                    break;
                case MoveType.StickToFloor:
                    t.position = Target.position.ZeroY() + Quaternion.AngleAxis(t.rotation.eulerAngles.y, Vector3.up)*m_PositionOffset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateRotation() {
            if (m_RotationType == RotationType.None) {
                return;
            }

            Transform t = transform;
            Quaternion targetRotation = GetTargetRotation();
            float angle = Quaternion.Angle(t.rotation, targetRotation);

            if (angle <= m_RotationSwim) {
                return;
            }

            float progress = (angle - m_RotationSwim)/angle;
            t.rotation = Quaternion.Lerp(t.rotation, targetRotation, progress);
        }

        private Quaternion GetTargetRotation() {
            Quaternion targetRotation;
            Vector3 targetAngles = Target.rotation.eulerAngles;

            switch (m_RotationType) {
                case RotationType.YOnly:
                    targetRotation = Quaternion.Euler(new Vector3(m_RotationOffset.x, m_RotationOffset.y + targetAngles.y, m_RotationOffset.z));
                    break;
                case RotationType.Full:
                    targetRotation = Quaternion.Euler(new Vector3(m_RotationOffset.x + targetAngles.x, m_RotationOffset.y + targetAngles.y, m_RotationOffset.z + targetAngles.z));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return targetRotation;
        }
    }
}