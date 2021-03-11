using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoreUtils {
    public class Delay : Singleton<Delay> {
        /// <summary>
        ///     Get a sequence that delays for 1 frame.
        /// </summary>
        /// <param name="context">The object doing the delaying. (Optional, for debug purposes.)</param>
        /// <returns>The same delay sequence (for chaining).</returns>
        public static DelaySequence OneFrame(Object context) {
            return ForFrameCount(1, context);
        }

        /// <summary>
        ///     Get a sequence that delays for a number of frames.
        /// </summary>
        /// <param name="frameCount">The number of frames to delay for.</param>
        /// <param name="context">The object doing the delaying. (Optional, for debug purposes.)</param>
        /// <returns>The same delay sequence (for chaining).</returns>
        public static DelaySequence ForFrameCount(int frameCount, Object context) {
            string info = $"{(context ? context.name + " " : "")}FrameDelay: {frameCount}";
            return new DelaySequence(context).ThenWaitForFrameCount(frameCount).WithName(info);
        }

        /// <summary>
        ///     Get a sequence that delays for a number of seconds.
        /// </summary>
        /// <param name="delay">The number of seconds to delay for.</param>
        /// <param name="context">The object doing the delaying. (Optional, for debug purposes.)</param>
        /// <returns>The same delay sequence (for chaining).</returns>
        public static DelaySequence For(float delay, Object context) {
            string info = $"{(context ? context.name + " " : "")}TimeDelay: {delay}";
            return new DelaySequence(context).ThenWaitFor(delay).WithName(info);
        }

        /// <summary>
        ///     Get a sequence that delays until a function returns true.
        /// </summary>
        /// <param name="predicate">The function to wait for.</param>
        /// <param name="context">The object doing the delaying. (Optional, for debug purposes.)</param>
        /// <returns>The same delay sequence (for chaining).</returns>
        public static DelaySequence Until(Func<bool> predicate, Object context) {
            string info = $"{(context ? context.name + " " : "")}WaitDelay: {predicate}";
            return new DelaySequence(context).ThenWaitUntil(predicate).WithName(info);
        }

        /// <summary>
        ///     Get a sequence that delays until a function returns true.
        /// </summary>
        /// <param name="predicate">The function to wait for.</param>
        /// <param name="timeOut">The number of seconds to wait before skipping the wait and finishing the promise anyway.</param>
        /// <param name="context">The object doing the delaying. (Optional, for debug purposes.)</param>
        /// <returns></returns>
        public static DelaySequence Until(Func<bool> predicate, float timeOut, Object context) {
            string info = $"{(context ? context.name + " " : "")}WaitDelay: {predicate}, timeout: {timeOut}";
            return new DelaySequence(context).ThenWaitUntil(predicate, timeOut).WithName(info);
        }

        /// <summary>
        ///     Get a delay sequence that delays until a sub-sequence completes.
        /// </summary>
        /// <param name="subSequenceFunc">The function that creates the sequence to wait for.</param>
        /// <param name="context">The object doing the delaying. (Optional, for debug purposes.)</param>
        /// <returns></returns>
        public static DelaySequence WaitFor(Func<DelaySequence> subSequenceFunc, Object context) {
            string info = $"{(context ? context.name + " " : "")}WaitFor: Sub-Sequence {subSequenceFunc}";
            return new DelaySequence(context).ThenWaitFor(subSequenceFunc).WithName(info);
        }

        /// <summary>
        ///     Get a delay sequence that delays until a sub-sequence completes.
        /// </summary>
        /// <param name="subSequenceFunc">The function that creates the sequence to wait for.</param>
        /// <param name="timeOut">The number of seconds to wait before skipping the wait and finishing the promise anyway.</param>
        /// <param name="context">The object doing the delaying. (Optional, for debug purposes.)</param>
        /// <returns></returns>
        public static DelaySequence WaitFor(Func<DelaySequence> subSequenceFunc, float timeOut, Object context) {
            string info = $"{(context ? context.name + " " : "")}WaitFor: Sub-Sequence {subSequenceFunc}, timeout: {timeOut}";
            return new DelaySequence(context).ThenWaitFor(subSequenceFunc, timeOut).WithName(info);
        }

        /// <summary>
        ///     Get a delay that must be manually completed.
        /// </summary>
        /// <param name="context">The object doing the delaying. (Optional, for debug purposes.)</param>
        /// <returns>The same delay sequence (for chaining).</returns>
        public static DelaySequence Manual(Object context) {
            string info = $"{(context ? context.name + " " : "")}ManualDelay: MUST COMPLETE MANUALLY";
            return new DelaySequence(context).ThenManualWait().WithName(info);
        }

        /// <summary>
        ///     This event raises when events are added or removed from the DelayEvents list.
        /// </summary>
        public event Action DelayEventsChanged;

        private int m_LastCount;

        /// <summary>
        ///     The list of delay events currently being evaluated.
        /// </summary>
        public List<DelaySequence> DelaySequences => m_DelaySequences ?? (m_DelaySequences = new List<DelaySequence>());

        private List<DelaySequence> m_DelaySequences;

        private void Start() {
            name = "DelayTracker";
            gameObject.hideFlags = HideFlags.DontSave;
            hideFlags = HideFlags.DontSave;
        }

        private void Update() {
            if (m_LastCount != DelaySequences.Count) {
                m_LastCount = DelaySequences.Count;
                RaiseOnDelayEventsChanged();
            }

            for (int i = DelaySequences.Count - 1; i >= 0; i--) {
                DelaySequences[i].Update();
            }

            DelaySequences.RemoveAll(s => s.IsDone);
        }

        private void OnDestroy() {
            DelaySequences.ForEach(d => d.Cancel("Delay object getting destroyed.", null));
        }

        public static void Add(DelaySequence sequence) {
            if (!AppTracker.IsPlaying) {
                return;
            }

            Instance.DelaySequences.Insert(0, sequence);
            Instance.RaiseOnDelayEventsChanged();
        }

        private void RaiseOnDelayEventsChanged() {
            DelayEventsChanged?.Invoke();
        }
    }
}