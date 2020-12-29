using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace CoreUtils.Editor {
    public class ProgressStepAttribute : Attribute {
        public ProgressStepAttribute(string description, float weight = 1.0f) {
            Description = description;
            Weight = weight;
        }

        public string Description { get; }

        public float Weight { get; }
    }

    public abstract class ProgressBar {
        public interface ITask {
            string Title { get; }
            float Start { get; }
            float Length { get; }
            void DisplayProgressBar(string title, string info, float progress);
            void Done();
        }

        private class SubTask : ITask {
            private readonly ProgressBar m_ProgressBar;

            public string Title => m_ProgressBar.Parent.Title;
            private string Description { get; }
            public float Start { get; }
            public float Length { get; }

            public SubTask(ProgressBar progressBar, string description, float start, float length) {
                m_ProgressBar = progressBar;
                Description = description;
                Start = start;
                Length = length;
            }

            public void DisplayProgressBar(string title, string info, float progress) {
                m_ProgressBar.Parent.DisplayProgressBar(title, info, progress);
            }

            public void Done() {
                DisplayProgressBar(Title, Description, Start + Length);
            }
        }

        private class RootTask : ITask {
            private readonly bool m_Cancelable;
            private double m_LastUpdateTime;
            private const double kMinPeriod = 0.1;

            public string Title { get; }
            public float Start => 0.0f;
            public float Length => 1.0f;

            public RootTask(string title, bool cancelable) {
                Title = title;
                m_Cancelable = cancelable;
                m_LastUpdateTime = float.NegativeInfinity;
            }

            public void DisplayProgressBar(string title, string info, float progress) {
                if (EditorApplication.timeSinceStartup >= m_LastUpdateTime + kMinPeriod) {
                    m_LastUpdateTime = EditorApplication.timeSinceStartup;
                    if (m_Cancelable) {
                        if (EditorUtility.DisplayCancelableProgressBar(title, info, progress)) {
                            throw new UserCancelledException();
                        }
                    } else {
                        EditorUtility.DisplayProgressBar(title, info, progress);
                    }
                }
            }

            public void Done() {
                EditorUtility.ClearProgressBar();
            }
        }

        private class SilentRootTask : ITask {
            public string Title => "";
            public float Start => 0.0f;
            public float Length => 1.0f;

            public void DisplayProgressBar(string title, string info, float progress) {
                // Do Nothing in Silent mode
            }

            public void Done() {
                // Do Nothing in Silent mode
            }
        }

        public class UserCancelledException : Exception { }

        protected ProgressBar(string description, bool cancelable, bool silent) {
            if (silent) {
                Parent = new SilentRootTask();
            } else {
                Parent = new RootTask(description, cancelable);
            }
        }

        protected ProgressBar(ITask parent) {
            Parent = parent;
        }

        private ITask Parent { get; }

        protected ITask StartTask(string description, float start, float length) {
            float globalStart = Parent.Start + start*Parent.Length;
            float globalLength = length*Parent.Length;
            string globalDescription = description;
            Parent.DisplayProgressBar(Parent.Title, globalDescription, globalStart);
            return new SubTask(this, globalDescription, globalStart, globalLength);
        }

        public void Done() {
            Parent.Done();
        }
    }

    public class ProgressBarEnum<T> : ProgressBar where T : IConvertible {
        private class StepData {
            public string Description { get; }
            public float StartProgress { get; private set; }
            public float Length { get; private set; }

            public StepData(string description, float startWeight, float weight) {
                Description = description;
                StartProgress = startWeight;
                Length = weight;
            }

            public void Normalize(float totalWeight) {
                StartProgress /= totalWeight;
                Length /= totalWeight;
            }
        }

        private Dictionary<int, StepData> m_StepMap;

        public ProgressBarEnum(string title, bool cancelable = false, bool silent = false)
            : base(title, cancelable, silent) {
            Init();
        }

        public ProgressBarEnum(ITask parent)
            : base(parent) {
            Init();
        }

        private void Init() {
            Type stepType = typeof(T);
            if (!stepType.IsEnum) {
                throw new ArgumentException("T must be an enum type");
            }

            m_StepMap = new Dictionary<int, StepData>();
            string[] names = Enum.GetNames(stepType);
            Array values = Enum.GetValues(stepType);

            float totalWeight = 0.0f;

            for (int i = 0; i < names.Length; i++) {
                int value = (int) values.GetValue(i);
                string name = names[i];
                string description = name;
                float weight = 1.0f;

                MemberInfo[] members = stepType.GetMember(names[i]);
                object[] attributes = members[0].GetCustomAttributes(typeof(ProgressStepAttribute), false);

                if (attributes.Length > 0 && attributes[0] is ProgressStepAttribute progressStep) {
                    description = progressStep.Description;
                    weight = progressStep.Weight;
                }

                m_StepMap[value] = new StepData(description, totalWeight, weight);
                totalWeight += weight;
            }

            foreach (StepData stepData in m_StepMap.Values) {
                stepData.Normalize(totalWeight);
            }
        }

        public ITask StartStep(T step) {
            int value = step.ToInt32(null);
            m_StepMap.TryGetValue(value, out StepData stepData);
            return StartTask(stepData.Description, stepData.StartProgress, stepData.Length);
        }
    }

    public class ProgressBarCounted : ProgressBar {
        private int m_NumSteps;

        public ProgressBarCounted(string title, int numSteps, bool cancelable = false, bool silent = false)
            : base(title, cancelable, silent) {
            Init(numSteps);
        }

        public ProgressBarCounted(ITask parent, int numSteps)
            : base(parent) {
            Init(numSteps);
        }

        private void Init(int numSteps) {
            m_NumSteps = numSteps;
        }

        public ITask StartStep(int step, string description) {
            return StartTask(description, step/(float) m_NumSteps, 1.0f/m_NumSteps);
        }
    }
}