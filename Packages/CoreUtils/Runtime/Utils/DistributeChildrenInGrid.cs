using System;
using UnityEngine;

namespace CoreUtils {
    public class DistributeChildrenInGrid : MonoBehaviour {
        [SerializeField] private float m_Distance = 0.5f;
        [SerializeField] private int m_Rows = 1;
        [SerializeField] private bool m_UpdateInEditor = true;

        private int m_LastCount;
        private int m_LastRows;
        private float m_LastDistance;

        private bool HasChanged => this && m_LastCount != transform.childCount || !m_Distance.Approximately(m_LastDistance) || m_LastRows != m_Rows;

        private void Update() {
            if (!HasChanged) {
                return;
            }

            Distribute();
        }

        private void Distribute() {
            m_LastCount = transform.childCount;
            m_LastDistance = m_Distance;
            m_LastRows = m_Rows;

            int columnCount = (int) Math.Ceiling(m_LastCount*1f/m_Rows);
            float halfDistanceColumn = m_Distance*(columnCount - 1)/2;
            float halfDistanceRow = m_Rows < m_LastCount ? m_Distance*(m_Rows - 1)/2 : m_Distance*(m_LastCount - 1)/2;

            for (int i = 0; i < m_LastCount; i++) {
                int row = i/columnCount;
                int column = i%columnCount;

                Transform child = transform.GetChild(i);
                Vector3 pos = new Vector3(column*m_Distance - halfDistanceColumn, 0, row*m_Distance - halfDistanceRow);
                child.localPosition = pos;
            }
        }

        private void OnValidate() {
            m_Rows = Math.Max(1, m_Rows);
            m_Distance = Math.Max(0, m_Distance);
            if (m_UpdateInEditor) {
                Distribute();
            }
        }
    }
}