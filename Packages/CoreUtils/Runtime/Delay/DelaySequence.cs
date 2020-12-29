using System;
using RSG;
using RSG.Exceptions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoreUtils {
    public class DelaySequence {
        private BaseDelay m_CurrentDelay;
        private Promise m_LastPromise;
        private readonly float m_StartTime = Time.time;
        private readonly float m_RealStartTime = Time.realtimeSinceStartup;
        private bool m_InRealTime;

        public string Name => m_CurrentDelay.Name;

        public bool IsDone => m_LastPromise.CurState != PromiseState.Pending;

        public Object Context { get; set; }

        private float CurrentTime => m_InRealTime ? Time.realtimeSinceStartup - m_RealStartTime : Time.time - m_StartTime;

        public static DelaySequence Empty => Delay.For(0, null);

        internal DelaySequence(Object context) {
            if (!Application.isPlaying) {
                throw new DelayPromiseException("Delay system only works when the application is running!");
            }

            Delay.Add(this);
            Context = context;
        }

        public DelaySequence WithName(string name) {
            m_LastPromise.WithName(name);
            return this;
        }

        public DelaySequence Then(Action onResolved) {
            m_LastPromise.Then(onResolved, ForceReject);
            return this;
        }

        public DelaySequence Then(Action onResolved, Action<Exception> onRejected) {
            m_LastPromise.Then(onResolved, onRejected);
            return this;
        }

        public DelaySequence ThenWaitFor(float duration) {
            return AddDelay(() => new TimeDelay(duration, this));
        }

        public DelaySequence ThenWaitForFrame() {
            return ThenWaitForFrameCount(1);
        }

        public DelaySequence ThenWaitForFrameCount(int frameCount) {
            return AddDelay(() => new FrameDelay(frameCount, this));
        }

        public DelaySequence ThenWaitUntil(Func<bool> test) {
            return AddDelay(() => new WaitDelay(test, this));
        }

        public DelaySequence ThenWaitUntil(Func<bool> test, float timeout) {
            return AddDelay(() => new WaitDelay(test, timeout, this));
        }

        public DelaySequence ThenWaitFor(Func<DelaySequence> subSequenceFunc) {
            return AddDelay(() => {
                try {
                    DelaySequence subSequence = subSequenceFunc();
                    return new WaitDelay(() => subSequence.IsDone, this);
                }
                catch (Exception e) {
                    Debug.LogException(e);
                    return new TimeDelay(0, this);
                }
            });
        }

        public DelaySequence ThenWaitFor(Func<DelaySequence> subSequenceFunc, float timeout) {
            return AddDelay(() => {
                DelaySequence subSequence = subSequenceFunc();
                return new WaitDelay(() => subSequence.IsDone, timeout, this);
            });
        }

        public DelaySequence ThenManualWait() {
            return AddDelay(() => new ManualDelay(this));
        }

        private DelaySequence AddDelay(Func<BaseDelay> getNextDelay) {
            if (m_CurrentDelay == null) {
                m_LastPromise = m_CurrentDelay = getNextDelay();
            } else {
                m_LastPromise = (Promise) m_LastPromise.Then(() => m_CurrentDelay = getNextDelay(), Reject);
            }

            return this;
        }

        public DelaySequence InRealTime() {
            if (CurrentTime > 0) {
                Debug.LogError("Can't set realtime after the sequence has started!", Context);
                return this;
            }

            m_InRealTime = true;
            return this;
        }

        public void Update() {
            if (IsDone) {
                return;
            }

            m_CurrentDelay.Update();
        }

        public void Complete() {
            if (IsDone) {
                return;
            }
            m_LastPromise.Resolve();
        }

        public void Cancel(string reason, Object context) {
            if (IsDone) {
                return;
            }

            string summary = "Cancelled '" + (Name.IsNullOrEmpty() && context ? context.name : Name) + "' delay sequence because: " + reason;
            Debug.Log(summary, context);
            Reject(new PromiseException(summary));
        }

        private void Reject(Exception ex) {
            if (m_CurrentDelay.CurState == PromiseState.Pending) {
                ForceReject(ex);
            }
        }

        private void ForceReject(Exception ex) {
            m_CurrentDelay.Reject(ex);
        }

        #region Helper Classes

        private abstract class BaseDelay : Promise {
            protected readonly DelaySequence m_Sequence;

            protected BaseDelay(DelaySequence sequence) {
                if (!Application.isPlaying) {
                    throw new DelayPromiseException("Delay system only works when the application is running!");
                }

                m_Sequence = sequence;
            }

            public void Update() {
                if (CurState == PromiseState.Pending && Check()) {
                    Resolve();
                }
            }

            protected abstract bool Check();
        }

        private class ManualDelay : BaseDelay {
            public ManualDelay(DelaySequence sequence) : base(sequence) { }

            protected override bool Check() {
                return false;
            }
        }

        private class WaitDelay : BaseDelay {
            private readonly float m_StartTime;
            private readonly float m_TimeOut;
            private readonly Func<bool> m_Test;

            private bool TimesUp => m_TimeOut >= 0 && m_Sequence.CurrentTime > m_StartTime + m_TimeOut;

            public WaitDelay(Func<bool> test, DelaySequence sequence) : this(test, -1, sequence) { }

            public WaitDelay(Func<bool> test, float timeOut, DelaySequence sequence) : base(sequence) {
                m_Test = test;
                m_StartTime = sequence.CurrentTime;
                m_TimeOut = timeOut;
            }

            protected override bool Check() {
                return m_Test() || TimesUp;
            }
        }

        private class TimeDelay : BaseDelay {
            private readonly float m_Delay;
            private readonly float m_StartTime;

            public TimeDelay(float delay, DelaySequence sequence) : base(sequence) {
                m_StartTime = sequence.CurrentTime;
                m_Delay = delay;
            }

            protected override bool Check() {
                return m_Sequence.CurrentTime >= m_StartTime + m_Delay;
            }
        }

        private class FrameDelay : BaseDelay {
            private readonly int m_GoFrame;

            public FrameDelay(int frameCount, DelaySequence sequence) : base(sequence) {
                m_GoFrame = Time.frameCount + frameCount;
            }

            protected override bool Check() {
                return Time.frameCount >= m_GoFrame;
            }
        }

        #endregion

    }
}