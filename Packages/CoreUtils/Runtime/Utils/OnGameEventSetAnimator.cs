using System;
using CoreUtils.GameEvents;
using UnityEngine;
using UnityEngine.Serialization;

namespace CoreUtils {
    public class OnGameEventSetAnimator : MonoBehaviour {
        public enum ParamType {
            Bool,
            Int,
            Float,
            Trigger
        }

        [SerializeField] private BaseGameEvent m_GameEvent;
        [SerializeField] private Animator m_Animator;
        [SerializeField] private ParamType m_Type;

        [FormerlySerializedAs("m_Name"), SerializeField]
        private string m_ParameterName;

        [SerializeField] private bool m_BoolValue;
        [SerializeField] private int m_IntValue;
        [SerializeField] private float m_FloatValue;

        public ParamType Type => m_Type;

        private void Awake() {
            m_Animator = m_Animator ? m_Animator : GetComponentInChildren<Animator>();

            if (!m_Animator) {
                Debug.LogWarningFormat(this, "No animator found.");
            }

            if (m_GameEvent != null) {
                m_GameEvent.GenericEvent += OnEvent;
            } else {
                Debug.LogWarningFormat(this, "GameEvent not set.");
            }
        }

        private void OnDestroy() {
            if (m_GameEvent != null) {
                m_GameEvent.GenericEvent -= OnEvent;
            }
        }

        private void OnEvent() {
            switch (m_Type) {
                case ParamType.Bool:
                    m_Animator.SetBool(m_ParameterName, m_BoolValue);
                    break;
                case ParamType.Int:
                    m_Animator.SetInteger(m_ParameterName, m_IntValue);
                    break;
                case ParamType.Float:
                    m_Animator.SetFloat(m_ParameterName, m_FloatValue);
                    break;
                case ParamType.Trigger:
                    m_Animator.SetTrigger(m_ParameterName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}