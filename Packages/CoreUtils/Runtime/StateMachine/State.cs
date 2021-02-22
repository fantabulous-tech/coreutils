using System;
using JetBrains.Annotations;
using UnityEngine;

namespace CoreUtils {
    public class State : MonoBehaviour {
        [SerializeField, AutoFillFromParent] private StateMachine m_StateMachine;

        private bool m_Init;
        
        public event Action OnEntered;
        public event Action OnExited;

        public bool IsFirst => transform.GetSiblingIndex() == 0;
        public bool IsLast => transform.GetSiblingIndex() == transform.parent.childCount - 1;

        public StateMachine StateMachine {
            get {
                if (m_StateMachine != null) {
                    return m_StateMachine;
                }

                m_StateMachine = transform.parent.GetComponent<StateMachine>();

                if (m_StateMachine == null) {
                    Debug.LogError("States must be the child of a StateMachine to operate.");
                    return null;
                }

                return m_StateMachine;
            }
        }

        private void Awake() {
            StateMachine.OnStateEntered += OnStateEntered;
            StateMachine.OnStateExited += OnStateExited;
        }

        private void OnDestroy() {
            if (StateMachine != null) {
                StateMachine.OnStateEntered -= OnStateEntered;
                StateMachine.OnStateExited -= OnStateExited;
            }
        }

        public void Init() {
            if (m_Init) {
                return;
            }

            m_Init = true;
            StateMachine.OnStateEntered += OnStateEntered;
            StateMachine.OnStateExited += OnStateExited;

            StateEvents events = GetComponent<StateEvents>();

            if (events) {
                events.Init();
            }

            RaiseOnExited();
        } 

        [UsedImplicitly]
        public void SetState() {
            StateMachine.ChangeState(name);
        }

        [UsedImplicitly]
        public void ChangeState(int childIndex) {
            StateMachine.ChangeState(childIndex);
        }

        [UsedImplicitly]
        public void ChangeState(GameObject state) {
            StateMachine.ChangeState(state.name);
        }

        [UsedImplicitly]
        public void ChangeState(string state) {
            if (StateMachine == null) {
                return;
            }
            StateMachine.ChangeState(state);
        }

        [UsedImplicitly]
        public void Next() {
            StateMachine.Next();
        }

        [UsedImplicitly]
        public void Previous() {
            StateMachine.Previous();
        }

        [UsedImplicitly]
        public void Exit() {
            StateMachine.Exit();
        }

        private void OnStateEntered(GameObject obj) {
            if (obj == gameObject) {
                RaiseOnEntered();
            }
        }

        private void OnStateExited(GameObject obj) {
            if (obj == gameObject) {
                RaiseOnExited();
            }
        }

        protected virtual void RaiseOnEntered() {
            OnEntered?.Invoke();
        }

        protected virtual void RaiseOnExited() {
            OnExited?.Invoke();
        }
    }
}