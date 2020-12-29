using System;
using UnityEngine;

namespace CoreUtils {
    public abstract class SetAnimatorBase : MonoBehaviour {
        [SerializeField] protected Animator m_Animator;
        [SerializeField] private float m_Delay;

        private DelaySequence m_DelaySequence;

        private void Awake() {
            m_Animator = m_Animator ? m_Animator : GetComponentInChildren<Animator>();
            if (!m_Animator) {
                Debug.LogWarningFormat(this, "No animator found.");
            }
        }

        protected void SetInternal(Action action) {
            if (m_DelaySequence != null) {
                m_DelaySequence.Cancel("Starting a new delay on SetAnimator", this);
            }

            if (m_Delay > 0) {
                m_DelaySequence = Delay.For(m_Delay, this).Then(action);
            } else {
                action();
            }
        }
    }
}