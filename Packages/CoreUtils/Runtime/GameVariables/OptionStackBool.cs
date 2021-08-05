using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CoreUtils.GameVariables {
    [CreateAssetMenu(menuName = "CoreUtils/Stack Variable Bool", order = (int) MenuOrder.VariableBool)]
    public class OptionStackBool : GameVariableBool {
        private readonly List<(Object, bool)> m_Pairs = new List<(Object, bool)>();

        protected override void SetValue(bool value) {
            Debug.LogError("OptionStack value set from somewhere.");
            base.SetValue(value);
        }

        public bool AddOption(Object controller, bool value) {
            (Object, bool) existingPair = m_Pairs.FirstOrDefault(p => p.Item1 == controller);

            if (existingPair.Item1) {
                Debug.LogWarning($"Existing pair for {controller.name} found. Resetting value to {value}.", this);
                m_Pairs.Remove(existingPair);
            }

            m_Pairs.Add((controller, value));
            UpdateValue();
            return Value;
        }

        public bool RemoveOption(Object controller) {
            (Object, bool) existingPair = m_Pairs.FirstOrDefault(p => p.Item1 == controller);

            if (existingPair.Item1) {
                m_Pairs.Remove(existingPair);
            } else {
                Debug.LogWarning($"Stack option not found. Can't remove {controller.name} from pair list.", this);
            }

            UpdateValue();
            return Value;
        }

        private void UpdateValue() {
            for (int i = m_Pairs.Count - 1; i >= 0; i--) {
                if (!m_Pairs[i].Item1) {
                    m_Pairs.RemoveAt(i);
                }
            }

            bool newValue = m_Pairs.Any() ? m_Pairs.Last().Item2 : m_InitialValue;

            if (newValue != Value) {
                base.SetValue(newValue);
                Raise();
            }
        }
    }
}