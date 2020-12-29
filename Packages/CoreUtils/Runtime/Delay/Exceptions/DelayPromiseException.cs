using System;

namespace CoreUtils {
    public class DelayPromiseException : Exception {
        public DelayPromiseException(string message) : base(message) { }
    }
}