using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace CoreUtils {
    public static class CollectionUtils {
        public static Dictionary<TKey, TValue> DictionaryFromLists<TKey, TValue>(IList<TKey> keys, IList<TValue> values) {
            if (keys == null || values == null) {
                return new Dictionary<TKey, TValue>();
            }

            // Both lists SHOULD be the same size. Clamp to the bounds of the smallest list.
            Debug.Assert(keys.Count == values.Count, string.Format("Key count ({0}) should be the same as value count ({1}).", keys.Count, values.Count));
            int bounds = Math.Min(keys.Count, values.Count);

            Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();
            for (int i = 0; i < bounds; i++) {
                dict[keys[i]] = values[i];
            }

            return dict;
        }

        public static T RandomItemFromCollection<T>(IList<T> list) {
            if (list == null || list.Count == 0) {
                return default;
            }

            int index = Random.Range(0, list.Count);
            return list[index];
        }

        public static List<TValue> ProduceListForKey<TKey, TValue>(Dictionary<TKey, List<TValue>> dictionary, TKey key) {
            if (dictionary == null) {
                return new List<TValue>();
            }

            dictionary.TryGetValue(key, out List<TValue> list);
            if (list == null) {
                list = new List<TValue>();
                dictionary.Add(key, list);
            }

            return list;
        }

        public static void ShuffleInto<T>(IEnumerable<T> source, IList<T> destination) {
            Debug.Assert(destination != null, "Destination cannot be null");
            if (source == null) {
                return;
            }

            foreach (T item in source) {
                ShuffleInto(item, destination);
            }
        }

        public static void ShuffleInto<T>(T item, IList<T> destination) {
            Debug.Assert(destination != null, "Destination cannot be null");

            if (item == null) {
                return;
            }

            // Fisher-Yates 'inside-out' shuffle
            // https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle#The_.22inside-out.22_algorithm
            int j = Random.Range(0, destination.Count + 1);
            if (j == destination.Count) {
                destination.Add(item);
            } else {
                destination.Add(destination[j]);
                destination[j] = item;
            }
        }

        public static IList<T> ShuffleToNewList<T>(IEnumerable<T> source) {
            IList<T> destination = new List<T>();
            ShuffleInto(source, destination);
            return destination;
        }

        public static void PruneDestroyed<T>(ICollection<T> collection) where T : Object {
            if (collection == null) {
                return;
            }

            List<T> toPrune = null;
            foreach (T item in collection) {
                if (item == null) {
                    if (toPrune == null) {
                        toPrune = new List<T>();
                    }

                    toPrune.Add(item);
                }
            }

            if (toPrune != null) {
                foreach (T item in toPrune) {
                    collection.Remove(item);
                }
            }
        }

        public static void PruneDestroyed<TKey, TValue>(IDictionary<TKey, TValue> dictionary) where TKey : Object {
            if (dictionary == null) {
                return;
            }

            List<TKey> toPrune = null;
            foreach (KeyValuePair<TKey, TValue> kvp in dictionary) {
                TKey item = kvp.Key;
                if (item == null) {
                    if (toPrune == null) {
                        toPrune = new List<TKey>();
                    }

                    toPrune.Add(item);
                }
            }

            if (toPrune != null) {
                foreach (TKey item in toPrune) {
                    dictionary.Remove(item);
                }
            }
        }

        public static T FindHighestScore<T>(IEnumerable<T> collection, Func<T, float> scoreFunc) where T : class {
            T best = null;
            float bestScore = float.MinValue;
            foreach (T candidate in collection) {
                float score = scoreFunc(candidate);
                if (best == null || score > bestScore) {
                    best = candidate;
                    bestScore = score;
                }
            }

            return best;
        }

        public static T GetFirstOrDefault<T>(this IList<T> list, T def) {
            if (list == null || list.Count == 0) {
                return def;
            }

            return list[0];
        }

        public static T GetFirst<T>(this IList<T> list) {
            return list.GetFirstOrDefault(default);
        }

        public static T GetLastOrDefault<T>(this IList<T> list, T def) {
            if (list == null || list.Count == 0) {
                return def;
            }

            return list[list.Count - 1];
        }

        public static T GetLast<T>(this IList<T> list) {
            return list.GetLastOrDefault(default);
        }

        public static bool Contains(this IList<string> list, string test, StringComparison comparisonType) {
            foreach (string item in list) {
                if (item.Equals(test, comparisonType)) {
                    return true;
                }
            }

            return false;
        }

        public static bool ContainsAny<T>(this IList<T> list, IEnumerable<T> items) {
            return items.Any(list.Contains);
        }

        public static void MoveUp<T>(this List<T> list, T item) {
            int index = list.IndexOf(item);

            if (index == 0) {
                Debug.LogWarning("Can't move above index '0'.");
                return;
            }

            list.Move(index, index - 1);
        }

        public static void MoveDown<T>(this List<T> list, T item) {
            int index = list.IndexOf(item);

            if (index == list.Count - 1) {
                Debug.LogWarning("Can't move past end of list.");
                return;
            }

            list.Move(index, index + 1);
        }

        public static void Move<T>(this IList<T> list, int oldIndex, int newIndex) {
            T item = list[oldIndex];
            list.RemoveAt(oldIndex);
            list.Insert(newIndex, item);
        }
    }
}