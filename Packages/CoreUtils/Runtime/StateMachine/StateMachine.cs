using System;
using UnityEngine;

namespace CoreUtils {
    public class StateMachine : MonoBehaviour {
        [SerializeField] private GameObject m_DefaultState;

        [Tooltip("Should log messages be thrown during usage?")]
        [SerializeField] private bool m_Verbose = true;

        [Tooltip("Can States within this StateMachine be reentered?")]
        [SerializeField] private bool m_AllowReentry;

        private GameObject m_CurrentState;

        public GameObject CurrentState {
            get {
                if (Application.isPlaying) {
                    return m_CurrentState;
                }

                for (int i = 0; i < transform.childCount; i++) {
                    GameObject child = transform.GetChild(i).gameObject;
                    if (child.activeSelf) {
                        return child;
                    }
                }

                return null;
            }
        }

        public GameObject FirstState => transform.childCount > 0 ? transform.GetChild(0).gameObject : null;
        public GameObject LastState => transform.childCount > 0 ? transform.GetChild(transform.childCount - 1).gameObject : null;

        public event Action<GameObject> OnStateEntered;
        public event Action<GameObject> OnStateExited;
        public event Action OnFirstStateEntered;
        public event Action OnFirstStateExited;
        public event Action OnLastStateEntered;
        public event Action OnLastStateExited;

        public void Awake() {
            // Initialize state visibility.
            foreach (Transform item in transform) {
                item.gameObject.SetActive(m_DefaultState == item.gameObject);
            }

            // Set first state explicitly.
            ChangeState(m_DefaultState);
        }

        public void Next() {
            if (CurrentState == null) {
                ChangeState(0);
                return;
            }
            int currentIndex = CurrentState.transform.GetSiblingIndex();
            if (currentIndex != transform.childCount - 1) {
                ChangeState(++currentIndex);
            }
        }

        public void Previous() {
            if (CurrentState == null) {
                ChangeState(0);
                return;
            }
            int currentIndex = CurrentState.transform.GetSiblingIndex();
            if (currentIndex == 0) {
                return;
            }
            ChangeState(--currentIndex);
        }

        public void Exit() {
            if (CurrentState == null) {
                return;
            }

            if (!Application.isPlaying) {
                foreach (Transform item in transform) {
                    item.gameObject.SetActive(false);
                }
                return;
            }

            Log($"(-) {name} EXITED state: {CurrentState.name}");
            int currentIndex = CurrentState.transform.GetSiblingIndex();

            //no longer at first:
            if (currentIndex == 0) {
                OnFirstStateExited?.Invoke();
            }

            //no longer at last:
            if (currentIndex == transform.childCount - 1) {
                OnLastStateExited?.Invoke();
            }

            OnStateExited?.Invoke(CurrentState);
            CurrentState.SetActive(false);
            m_CurrentState = null;
        }

        public void ChangeState(int childIndex) {
            if (childIndex > transform.childCount - 1) {
                LogWarning($"Index is greater than the amount of states in the StateMachine \"{gameObject.name}\" please verify the index you are trying to change to.");
                return;
            }

            if (childIndex < 0) {
                Exit();
            } else {
                ChangeState(transform.GetChild(childIndex).gameObject);
            }
        }

        public void ChangeState(GameObject state) {
            if (state == null) {
                Exit();
                return;
            }

            if (!Application.isPlaying) {
                foreach (Transform item in transform) {
                    item.gameObject.SetActive(item.gameObject == state);
                }
                return;
            }

            if (CurrentState != null) {
                if (!m_AllowReentry && state == CurrentState) {
                    Log($"State change ignored. State machine \"{name}\" already in \"{state.name}\" state.");
                    return;
                }
            }

            if (state.transform.parent != transform) {
                LogWarning($"State \"{state.name}\" is not a child of \"{name}\" StateMachine state change canceled.");
                return;
            }

            Exit();
            Enter(state);
        }

        public void ChangeState(string state) {
            if (state.IsNullOrEmpty()) {
                Exit();
                return;
            }

            Transform found = transform.Find(state);

            if (found) {
                ChangeState(found.gameObject);
            } else {
                LogWarning($"\"{name}\" does not contain a state by the name of \"{state}\" please verify the name of the state you are trying to reach.");
            }
        }

        private void Enter(GameObject state) {
            m_CurrentState = state;
            int index = m_CurrentState.transform.GetSiblingIndex();

            if (index == 0) {
                OnFirstStateEntered?.Invoke();
            }

            if (index == transform.childCount - 1) {
                OnLastStateEntered?.Invoke();
            }

            Log($"(+) {name} ENTERED state: {state.name}");
            OnStateEntered?.Invoke(m_CurrentState);
            m_CurrentState.SetActive(true);
        }

        private void Log(string message) {
            if (m_Verbose) {
                Debug.Log(message, this);
            }
        }

        private void LogWarning(string message) {
            Debug.LogWarning(message, this);
        }
    }
}