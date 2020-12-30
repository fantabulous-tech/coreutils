using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace CoreUtils {
    [Serializable, DebuggerDisplay("Count = {Count}")]
    public class SerializableDictionary<TKey, TValue> : IDictionary<TKey, TValue> {
        [SerializeField, HideInInspector] private int[] m_Buckets;
        [SerializeField, HideInInspector] private int[] m_HashCodes;
        [SerializeField, HideInInspector] private int[] m_Next;
        [SerializeField, HideInInspector] private int m_Count;
        [SerializeField, HideInInspector] private int m_Version;
        [SerializeField, HideInInspector] private int m_FreeList;
        [SerializeField, HideInInspector] private int m_FreeCount;
        [SerializeField, HideInInspector] private TKey[] m_Keys;
        [SerializeField, HideInInspector] private TValue[] m_Values;

        private readonly IEqualityComparer<TKey> m_Comparer;

        // Mainly for debugging purposes - to get the key-value pairs display
        public Dictionary<TKey, TValue> AsDictionary => new Dictionary<TKey, TValue>(this);

        public int Count => m_Count - m_FreeCount;

        public TValue this[TKey key, TValue defaultValue] {
            get {
                int index = FindIndex(key);
                if (index >= 0) {
                    return m_Values[index];
                }
                return defaultValue;
            }
        }

        public TValue this[TKey key] {
            get {
                int index = FindIndex(key);
                if (index >= 0) {
                    return m_Values[index];
                }
                throw new KeyNotFoundException(key.ToString());
            }
            set => Insert(key, value, false);
        }

        public SerializableDictionary() : this(0, null) { }

        public SerializableDictionary(int capacity) : this(capacity, null) { }

        public SerializableDictionary(IEqualityComparer<TKey> comparer) : this(0, comparer) { }

        public SerializableDictionary(int capacity, IEqualityComparer<TKey> comparer) {
            if (capacity < 0) {
                throw new ArgumentOutOfRangeException("capacity");
            }

            Initialize(capacity);

            m_Comparer = comparer ?? EqualityComparer<TKey>.Default;
        }

        public SerializableDictionary(IDictionary<TKey, TValue> dictionary) : this(dictionary, null) { }

        public SerializableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : this(dictionary != null ? dictionary.Count : 0, comparer) {
            if (dictionary == null) {
                throw new ArgumentNullException("dictionary");
            }

            foreach (KeyValuePair<TKey, TValue> current in dictionary) {
                Add(current.Key, current.Value);
            }
        }

        public bool ContainsValue(TValue value) {
            if (value == null) {
                for (int i = 0; i < m_Count; i++) {
                    if (m_HashCodes[i] >= 0 && m_Values[i] == null) {
                        return true;
                    }
                }
            } else {
                EqualityComparer<TValue> defaultComparer = EqualityComparer<TValue>.Default;
                for (int i = 0; i < m_Count; i++) {
                    if (m_HashCodes[i] >= 0 && defaultComparer.Equals(m_Values[i], value)) {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool ContainsKey(TKey key) {
            return FindIndex(key) >= 0;
        }

        public void Clear() {
            if (m_Count <= 0) {
                return;
            }

            for (int i = 0; i < m_Buckets.Length; i++) {
                m_Buckets[i] = -1;
            }

            Array.Clear(m_Keys, 0, m_Count);
            Array.Clear(m_Values, 0, m_Count);
            Array.Clear(m_HashCodes, 0, m_Count);
            Array.Clear(m_Next, 0, m_Count);

            m_FreeList = -1;
            m_Count = 0;
            m_FreeCount = 0;
            m_Version++;
        }

        public void Add(TKey key, TValue value) {
            Insert(key, value, true);
        }

        private void Resize(int newSize, bool forceNewHashCodes) {
            int[] bucketsCopy = new int[newSize];
            for (int i = 0; i < bucketsCopy.Length; i++) {
                bucketsCopy[i] = -1;
            }

            TKey[] keysCopy = new TKey[newSize];
            TValue[] valuesCopy = new TValue[newSize];
            int[] hashCodesCopy = new int[newSize];
            int[] nextCopy = new int[newSize];

            Array.Copy(m_Values, 0, valuesCopy, 0, m_Count);
            Array.Copy(m_Keys, 0, keysCopy, 0, m_Count);
            Array.Copy(m_HashCodes, 0, hashCodesCopy, 0, m_Count);
            Array.Copy(m_Next, 0, nextCopy, 0, m_Count);

            if (forceNewHashCodes) {
                for (int i = 0; i < m_Count; i++) {
                    if (hashCodesCopy[i] != -1) {
                        hashCodesCopy[i] = m_Comparer.GetHashCode(keysCopy[i]) & 2147483647;
                    }
                }
            }

            for (int i = 0; i < m_Count; i++) {
                int index = hashCodesCopy[i]%newSize;
                nextCopy[i] = bucketsCopy[index];
                bucketsCopy[index] = i;
            }

            m_Buckets = bucketsCopy;
            m_Keys = keysCopy;
            m_Values = valuesCopy;
            m_HashCodes = hashCodesCopy;
            m_Next = nextCopy;
        }

        private void Resize() {
            Resize(PrimeHelper.ExpandPrime(m_Count), false);
        }

        public bool Remove(TKey key) {
            if (key == null) {
                throw new ArgumentNullException("key");
            }

            int hash = m_Comparer.GetHashCode(key) & 2147483647;
            int index = hash%m_Buckets.Length;
            int num = -1;
            for (int i = m_Buckets[index]; i >= 0; i = m_Next[i]) {
                if (m_HashCodes[i] == hash && m_Comparer.Equals(m_Keys[i], key)) {
                    if (num < 0) {
                        m_Buckets[index] = m_Next[i];
                    } else {
                        m_Next[num] = m_Next[i];
                    }

                    m_HashCodes[i] = -1;
                    m_Next[i] = m_FreeList;
                    m_Keys[i] = default;
                    m_Values[i] = default;
                    m_FreeList = i;
                    m_FreeCount++;
                    m_Version++;
                    return true;
                }
                num = i;
            }
            return false;
        }

        private void Insert(TKey key, TValue value, bool add) {
            if (key == null) {
                throw new ArgumentNullException("key");
            }

            if (m_Buckets == null) {
                Initialize(0);
            }

            int hash = m_Comparer.GetHashCode(key) & 2147483647;
            int index = hash%m_Buckets.Length;
            for (int i = m_Buckets[index]; i >= 0; i = m_Next[i]) {
                if (m_HashCodes[i] == hash && m_Comparer.Equals(m_Keys[i], key)) {
                    if (add) {
                        throw new ArgumentException("Key already exists: " + key);
                    }

                    m_Values[i] = value;
                    m_Version++;
                    return;
                }
            }
            int num;
            if (m_FreeCount > 0) {
                num = m_FreeList;
                m_FreeList = m_Next[num];
                m_FreeCount--;
            } else {
                if (m_Count == m_Keys.Length) {
                    Resize();
                    index = hash%m_Buckets.Length;
                }
                num = m_Count;
                m_Count++;
            }
            m_HashCodes[num] = hash;
            m_Next[num] = m_Buckets[index];
            m_Keys[num] = key;
            m_Values[num] = value;
            m_Buckets[index] = num;
            m_Version++;
        }

        private void Initialize(int capacity) {
            int prime = PrimeHelper.GetPrime(capacity);

            m_Buckets = new int[prime];
            for (int i = 0; i < m_Buckets.Length; i++) {
                m_Buckets[i] = -1;
            }

            m_Keys = new TKey[prime];
            m_Values = new TValue[prime];
            m_HashCodes = new int[prime];
            m_Next = new int[prime];

            m_FreeList = -1;
        }

        private int FindIndex(TKey key) {
            if (key == null) {
                throw new ArgumentNullException("key");
            }

            if (m_Buckets != null) {
                int hash = m_Comparer.GetHashCode(key) & 2147483647;
                for (int i = m_Buckets[hash%m_Buckets.Length]; i >= 0; i = m_Next[i]) {
                    if (m_HashCodes[i] == hash && m_Comparer.Equals(m_Keys[i], key)) {
                        return i;
                    }
                }
            }
            return -1;
        }

        public bool TryGetValue(TKey key, out TValue value) {
            int index = FindIndex(key);
            if (index >= 0) {
                value = m_Values[index];
                return true;
            }
            value = default;
            return false;
        }

        public ICollection<TKey> Keys => m_Keys.Take(Count).ToArray();

        public ICollection<TValue> Values => m_Values.Take(Count).ToArray();

        public void Add(KeyValuePair<TKey, TValue> item) {
            Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) {
            int index = FindIndex(item.Key);
            return index >= 0 && EqualityComparer<TValue>.Default.Equals(m_Values[index], item.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            if (array == null) {
                throw new ArgumentNullException("array");
            }

            if (arrayIndex < 0 || arrayIndex > array.Length) {
                throw new ArgumentOutOfRangeException(string.Format("index = {0} array.Length = {1}", arrayIndex, array.Length));
            }

            if (array.Length - arrayIndex < Count) {
                throw new ArgumentException(
                    string.Format(
                        "The number of elements in the dictionary ({0}) is greater than the available space from index to the end of the destination array {1}.",
                        Count, array.Length));
            }

            for (int i = 0; i < m_Count; i++) {
                if (m_HashCodes[i] >= 0) {
                    array[arrayIndex++] = new KeyValuePair<TKey, TValue>(m_Keys[i], m_Values[i]);
                }
            }
        }

        public bool IsReadOnly => false;

        public bool Remove(KeyValuePair<TKey, TValue> item) {
            return Remove(item.Key);
        }

        public Enumerator GetEnumerator() {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() {
            return GetEnumerator();
        }

        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>> {
            private readonly SerializableDictionary<TKey, TValue> m_Dictionary;
            private readonly int m_Version;
            private int m_Index;

            public KeyValuePair<TKey, TValue> Current { get; private set; }

            internal Enumerator(SerializableDictionary<TKey, TValue> dictionary) {
                m_Dictionary = dictionary;
                m_Version = dictionary.m_Version;
                Current = default;
                m_Index = 0;
            }

            public bool MoveNext() {
                if (m_Version != m_Dictionary.m_Version) {
                    throw new InvalidOperationException(string.Format("Enumerator version {0} != Dictionary version {1}", m_Version,
                                                                      m_Dictionary.m_Version));
                }

                while (m_Index < m_Dictionary.m_Count) {
                    if (m_Dictionary.m_HashCodes[m_Index] >= 0) {
                        Current = new KeyValuePair<TKey, TValue>(m_Dictionary.m_Keys[m_Index], m_Dictionary.m_Values[m_Index]);
                        m_Index++;
                        return true;
                    }
                    m_Index++;
                }

                m_Index = m_Dictionary.m_Count + 1;
                Current = default;
                return false;
            }

            void IEnumerator.Reset() {
                if (m_Version != m_Dictionary.m_Version) {
                    throw new InvalidOperationException(string.Format("Enumerator version {0} != Dictionary version {1}", m_Version,
                                                                      m_Dictionary.m_Version));
                }

                m_Index = 0;
                Current = default;
            }

            object IEnumerator.Current => Current;

            public void Dispose() {
                //Needed for interface.
            }
        }
    }
}