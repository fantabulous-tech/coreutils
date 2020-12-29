using System;
using UnityEngine;

namespace CoreUtils.UI {
    public class FadeCanvasGroup : MonoBehaviour {
        [SerializeField] private CanvasGroup m_CanvasGroup;
        [SerializeField] private bool m_FadeOnEnable;
        [SerializeField] private float m_FadeDuration = 1;
        [SerializeField] private AnimationCurve m_FadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private FadeDirection m_Direction;

        private enum FadeDirection {
            In,
            Out
        }

        private float m_CurrentFadeDuration;
        private AnimationCurve m_CurrentFadeCurve;
        private float m_FadeStartTime;
        private float m_FadeStart;
        private bool m_InTransition;
        private float m_FadeTarget;
        private DelaySequence m_Delay;

        private float FadeProgress => (Time.time - m_FadeStartTime)/m_CurrentFadeDuration;
        private bool FadeComplete => FadeProgress >= 1;

        private CanvasGroup CanvasGroup => UnityUtils.GetOrSet(ref m_CanvasGroup, GetComponentInChildren<CanvasGroup>);

        private float Alpha {
            get => CanvasGroup.alpha;
            set => CanvasGroup.alpha = value;
        }

        private void Awake() {
            Alpha = 0;
        }

        private void OnEnable() {
            if (!m_FadeOnEnable) {
                return;
            }

            switch (m_Direction) {
                case FadeDirection.In:
                    Alpha = 0;
                    In(m_FadeDuration, m_FadeCurve);
                    break;
                case FadeDirection.Out:
                    Alpha = 1;
                    Out(m_FadeDuration, m_FadeCurve);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void LateUpdate() {
            UpdateFade();
        }

        private void Reset() {
            m_CanvasGroup = GetComponent<CanvasGroup>();
        }

        private void UpdateFade() {
            if (!m_InTransition) {
                return;
            }
            if (!FadeComplete) {
                Alpha = Mathf.Lerp(m_FadeStart, m_FadeTarget, m_CurrentFadeCurve.Evaluate(m_FadeTarget.Approximately(0) ? 1 - FadeProgress : FadeProgress));
            } else {
                CompleteFade();
            }
        }

        private void CompleteFade() {
            Alpha = m_FadeTarget;
            m_InTransition = false;
            m_Delay.Complete();
        }

        public DelaySequence In(float duration, AnimationCurve curve) {
            return To(1, duration, curve);
        }

        public DelaySequence Out(float duration, AnimationCurve curve) {
            return duration.Approximately(0) ? Hide() : To(0, duration, curve);
        }

        public DelaySequence Hide() {
            m_FadeTarget = 0;
            CompleteFade();
            return DelaySequence.Empty;
        }

        private DelaySequence To(float target, float duration, AnimationCurve curve) {
            m_FadeTarget = target;
            m_CurrentFadeDuration = duration;
            m_CurrentFadeCurve = curve;
            m_FadeStartTime = Time.time;
            m_FadeStart = Alpha;
            m_InTransition = true;
            return m_Delay = Delay.Manual(this);
        }
    }
}