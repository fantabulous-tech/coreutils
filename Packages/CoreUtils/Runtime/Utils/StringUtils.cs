using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace CoreUtils {
    public static class StringUtils {
        /// <summary>
        ///     Returns true if the <paramref name="text" /> string contans the <paramref name="search" /> string using the
        ///     supplied
        ///     <paramref name="comparisonType" />.
        /// </summary>
        /// <param name="text">The string to test for the check string.</param>
        /// <param name="search">The check string.</param>
        /// <param name="comparisonType">The comparison method to use which allows for IgnoreCase methods.</param>
        /// <returns>True if the search string is found in the given text.</returns>
        public static bool Contains(this string text, string search, StringComparison comparisonType) {
            if (search.IsNullOrEmpty()) {
                return true;
            }

            if (text.IsNullOrEmpty()) {
                return false;
            }

            return text.IndexOf(search, comparisonType) >= 0;
        }

        /// <summary>
        ///     Returns true if the strings is null or an empty string.
        /// </summary>
        /// <param name="source">The string to test.</param>
        /// <returns>True if the string is null or empty, otherwise false.</returns>
        [ContractAnnotation("source:null => true")]
        public static bool IsNullOrEmpty(this string source) {
            return string.IsNullOrEmpty(source);
        }

        public static string[] Split(this string str, string sep) {
            return str.Split(new[] {sep}, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string AggregateToString<T>(this IEnumerable<T> ie, Func<T, string> converter, string prepend = "", string append = "") {
            return AggregateToString(ie, ", ", converter, prepend, append);
        }

        public static string AggregateToString<T>(this IEnumerable<T> strings, string sep = ", ", Func<T, string> converter = null, string prepend = "", string append = "") {
            if (converter == null) {
                converter = v => v.ToString();
            }

            return strings.Aggregate(new StringBuilder(prepend), (c, n) => {
                if (c.Length > prepend.Length) {
                    return c.Append(sep).Append(converter(n));
                }

                return c.Append(converter(n));
            }).Append(append).ToString();
        }

        /// <summary>
        ///     A convenience extension version of Regex.Replace.
        /// </summary>
        /// <param name="input">The string to search for a match.</param>
        /// <param name="pattern">The regular expression pattern to match.</param>
        /// <param name="options">A bitwise combination of the enumeration values that provide options for matching.</param>
        /// <returns>
        ///     A new string that is identical to the input string, except that the replacement string takes the place of each
        ///     matched string.
        /// </returns>
        public static bool ContainsRegex(this string input, string pattern, RegexOptions options = RegexOptions.None) {
            return Regex.Match(input, pattern, options).Success;
        }

        /// <summary>
        ///     A convenience extension version of Regex.Replace.
        /// </summary>
        /// <param name="input">The string to search for a match.</param>
        /// <param name="pattern">The regular expression pattern to match.</param>
        /// <param name="replacement">The replacement string.</param>
        /// <param name="options">A bitwise combination of the enumeration values that provide options for matching.</param>
        /// <returns>
        ///     A new string that is identical to the input string, except that the replacement string takes the place of each
        ///     matched string.
        /// </returns>
        public static string ReplaceRegex(this string input, string pattern, string replacement, RegexOptions options = RegexOptions.None) {
            return Regex.Replace(input, pattern, replacement, options);
        }

        /// <summary>
        ///     A convenience extension version of Regex.Split.
        /// </summary>
        /// <param name="input">The string to search for a match.</param>
        /// <param name="pattern">The regular expression pattern to match.</param>
        /// <param name="options">A bitwise combination of the enumeration values that provide options for matching.</param>
        /// <returns>
        ///     A new array of strings split based on the string and pattern given.
        /// </returns>
        public static string[] SplitRegex(this string input, string pattern, RegexOptions options = RegexOptions.None) {
            return Regex.Split(input, pattern, options);
        }

        /// <summary>
        ///     Replaces a string's old value with a new value using the string comparison type.
        /// </summary>
        /// <param name="originalString">The string to run the search/replace on.</param>
        /// <param name="oldValue">The old value to find.</param>
        /// <param name="newValue">The new value to replace.</param>
        /// <param name="comparisonType">The type of comparison to use.</param>
        /// <returns></returns>
        public static string Replace(this string originalString, string oldValue, string newValue, StringComparison comparisonType) {
            int startIndex = 0;
            while (true) {
                startIndex = originalString.IndexOf(oldValue, startIndex, comparisonType);
                if (startIndex == -1) {
                    break;
                }

                originalString = originalString.Substring(0, startIndex) + newValue + originalString.Substring(startIndex + oldValue.Length);
                startIndex += newValue.Length;
            }

            return originalString;
        }

        /// <summary>
        ///     Parses out a string (e.g. file name or camel case ID) into a readable name.
        /// </summary>
        /// <param name="text">The text to convert.</param>
        /// <param name="capitalize">If true, will capitalize the first character of words.</param>
        /// <param name="removeNumbers">If true, will remove numbers.</param>
        /// <param name="removeSingleLetters">If true, removes individual letters.</param>
        /// <returns>The converted human-readable string.</returns>
        public static string ToSpacedName(this string text, bool capitalize = true, bool removeNumbers = true, bool removeSingleLetters = true) {
            text = text.ReplaceRegex(@"[A-Z][a-z]", " $0").ReplaceRegex(@"([0-9])([A-Za-z])|([A-Za-z])([0-9])", "$1$3 $2$4");

            text = RemoveSymbols(text, removeSingleLetters);

            if (removeNumbers) {
                text = RemoveNumbers(text, removeSingleLetters);
            } else if (removeSingleLetters) {
                text = RemoveSingleLetters(text);
            }

            return capitalize ? text.ToCapitalized().Trim() : text.Trim();
        }

        private static string RemoveSymbols(string text, bool removeSingleLetters) {
            string[] testArray = text.Split(new[] {' ', '\t', '.', '_', '-', '#', '$', '%'}, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < testArray.Length; i++) {
                if (testArray[i].Length <= 1 && removeSingleLetters) {
                    continue;
                }

                if (sb.Length != 0) {
                    sb.Append(" ");
                }

                sb.Append(testArray[i]);
            }

            return sb.ToString();
        }

        private static string RemoveNumbers(string text, bool removeSingleLetters) {
            string[] testArray = text.Split(new[] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'}, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < testArray.Length; i++) {
                if (testArray[i].Length <= 1 && removeSingleLetters) {
                    continue;
                }

                if (sb.Length != 0) {
                    sb.Append(" ");
                }

                sb.Append(testArray[i].Trim());
            }

            return sb.ToString();
        }

        private static string RemoveSingleLetters(string text) {
            string[] testArray = text.Split(new[] {' ', '\t', '.', '_', '-', '#', '$', '%'}, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < testArray.Length; i++) {
                if (testArray[i].Length == 1 && !char.IsDigit(testArray[i][0])) {
                    continue;
                }

                if (sb.Length != 0) {
                    sb.Append(" ");
                }

                sb.Append(testArray[i]);
            }

            return sb.ToString();
        }

        /// <summary>
        ///     Capitalizes the first letter of each word in the supplied string.
        /// </summary>
        /// <param name="text">The text to capitalize.</param>
        /// <param name="invariant">If true, will captialize in the same way regardless of current culture.</param>
        /// <returns>The text with capitalized first characters.</returns>
        public static string ToCapitalized(this string text, bool invariant = false) {
            return invariant ? CultureInfo.InvariantCulture.TextInfo.ToTitleCase(text) : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text);
        }

        /// <summary>
        ///     Capitalizes the first letter of each word except words normally lower case in titles.
        /// </summary>
        /// <param name="text">The text to change to title case.</param>
        /// <returns>The text converted to title case.</returns>
        public static string ToTitleCase(this string text) {
            // Cycle through all words
            foreach (Match match in Regex.Matches(text, @"\w+", RegexOptions.IgnoreCase)) {
                // Set replacement words to lowercase
                string prefix = text.Substring(0, match.Index);
                string change = Regex.IsMatch(match.Value, @"\b(a|an|the|at|by|for|in|of|on|to|up|and|as|but|it|or|nor|with)\b", RegexOptions.IgnoreCase) ? match.Value.ToLower() : match.Value.ToCapitalized();
                string postFix = text.Substring(match.Index + match.Length);

                text = prefix + change + postFix;
            }

            return text;
        }

        public static string ToUnderscored(this string text) {
            return Regex.Replace(text, @"(?:(?<!\b)([A-Z])|\s+)", m => "_" + (m.Captures.Count > 1 ? m.Captures[1].ToString().ToLower() : ""), RegexOptions.Singleline).ToLower();
        }

        public static string ToKebab(this string text) {
            return Regex.Replace(text, @"(?:(?<!\b)([A-Z])|\s+)", m => "-" + (m.Captures.Count > 1 ? m.Captures[1].ToString().ToLower() : ""), RegexOptions.Singleline).ToLower();
        }

        public static string FromCamel(this string text) {
            return Regex.Replace(text, "(?<=[a-z])([A-Z0-9])", " $1").ToCapitalized(true);
        }

        public static string ToCamel(this string text, bool firstLower = false) {
            text = text.Trim();
            if (text.Length == 0) {
                return "";
            }

            if (text.Length == 1) {
                return text.ToLowerInvariant();
            }

            string camel = Regex.Replace(text, "[\\W_]+", " ", RegexOptions.Singleline);
            camel = Regex.Replace(camel, "(.)([A-Z][a-z])", "$1 $2");
            camel = camel.ToCapitalized(true).Replace(" ", "");
            if (camel.Length == 0) {
                return "";
            }

            if (camel.Length == 1) {
                return camel.ToLowerInvariant();
            }

            if (firstLower) {
                return camel.Substring(0, 1).ToLowerInvariant() + camel.Substring(1);
            }

            return camel;
        }
    }
}
