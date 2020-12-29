using System;
using CoreUtils.GameEvents;
using UnityEngine;

namespace CoreUtils.GameVariables {
    public abstract class BaseGameVariable : BaseGameEvent {
        public abstract string ValueString { get; set; }
        public abstract void ResetValue();
    }

    public abstract class BaseGameVariable<thisT, T> : BaseGameVariable where thisT : BaseGameVariable<thisT, T> {
        [NonSerialized] protected T m_CurrentValue;
        [SerializeField] protected T m_InitialValue;
        [NonSerialized] private T m_LastValue;

        public T Value {
            get {
                TryInit();
                return GetValue();
            }
            set {
                TryInit();
                if (Equals(GetValue(), value)) {
                    return;
                }
                SetValue(value);
                Raise();
            }
        }

        public override string ValueString {
            get => GetValue() == null ? "--null--" : GetValue().ToString();
            set => Value = Parse(value);
        }

        public event Action<T> Changed;

        protected virtual void SetValue(T value) {
            m_CurrentValue = value;
        }

        protected virtual T GetValue() {
            return m_CurrentValue;
        }

        protected abstract T Parse(string stringValue);

        public override void ResetValue() {
            SetValue(m_InitialValue);
        }

        protected virtual bool Equals(T a, T b) {
            return object.Equals(a, b);
        }

        protected override void Init() {
            base.Init();
            m_LastValue = m_CurrentValue = m_InitialValue;
        }

        protected override void RaiseDefault() {
            DataInfo = string.Format("({0})", ValueString);

            RaiseGeneric();

            if (Changed != null) {
                try {
                    Changed(Value);
                }
                catch (Exception e) {
                    Debug.LogException(e);
                }
            }
        }

        protected virtual void OnValidate() {
            if (Equals(m_CurrentValue, m_LastValue)) {
                return;
            }
            m_LastValue = m_CurrentValue;
            if (Application.isPlaying) {
                Raise();
            }
        }
    }
}