using System;
using CoreUtils.GameVariables;
using TMPro;
using UnityEngine;

namespace CoreUtils.UI {
    public class DropDownBinding : MonoBehaviour {
        [SerializeField, AutoFillAsset] private BaseGameVariable m_Variable;
        [SerializeField, AutoFill] private TMP_Dropdown m_Dropdown;

        public UnityEventString OnChanged;

        private void OnEnable() {
            m_Variable.GenericEvent += OnVariableChanged;
            m_Dropdown.onValueChanged.AddListener(OnDropdownChanged);
            OnVariableChanged();
        }

        private void OnDisable() {
            if (m_Variable != null) {
                m_Variable.GenericEvent -= OnVariableChanged;
            }

            if (m_Dropdown) {
                m_Dropdown.onValueChanged.RemoveListener(OnDropdownChanged);
            }
        }

        private void OnDropdownChanged(int optionIndex) {
            m_Variable.ValueString = m_Dropdown.options[optionIndex].text;
        }

        private void OnVariableChanged() {
            UpdateDropdown();
            OnChanged.Invoke(m_Variable.ValueString);
        }

        private void OnValidate() {
            UpdateDropdown();
        }

        private void UpdateDropdown() {
            if (!m_Dropdown || !m_Variable) {
                return;
            }

            int dropdownIndex = m_Dropdown.options.IndexOf(o => o.text.Equals(m_Variable.ValueString, StringComparison.OrdinalIgnoreCase));

            if (dropdownIndex >= 0) {
                m_Dropdown.value = dropdownIndex;
            } else {
                Debug.LogWarning($"Couldn't select dropdown value {m_Variable.ValueString} because it's not in the dropdown values.", this);
            }
        }
    }
}