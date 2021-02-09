using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace RemoteController.Core
{
    public class ObservableHashTable<TKey, TValue> : System.Windows.DependencyObject, IEnumerable<TValue>, INotifyCollectionChanged
    {
        public struct Entry
        {
            public int HashCode;    // Lower 31 bits of hash code, -1 if unused
            public int Next;        // Index of next entry, -1 if last
            public TKey Key;           // Key of entry
            public TValue Value;         // Value of entry
        }

        private int[] buckets;
        private Entry[] entries;
        private int count;
        private readonly object _syncRoot = new object();

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public ObservableHashTable() : this(0, null) { }

        public ObservableHashTable(int capacity) : this(capacity, null) { }

        public ObservableHashTable(IEqualityComparer<TKey> comparer) : this(0, comparer) { }

        public ObservableHashTable(int capacity, IEqualityComparer<TKey> comparer)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));
            if (capacity > 0)
                Initialize(capacity);
            Comparer = comparer ?? EqualityComparer<TKey>.Default;
        }

        public ObservableHashTable(IDictionary<TKey, TValue> dictionary) : this(dictionary, null) { }

        public ObservableHashTable(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) :
            this(dictionary != null ? dictionary.Count : 0, comparer)
        {

            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            {
                Add(pair.Key, pair.Value);
            }
        }

        public IReadOnlyCollection<Entry> Entries
        {
            get => entries;
        }

        public IEqualityComparer<TKey> Comparer { get; }

        public int Count
        {
            get { return count; }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                TKey[] keys = new TKey[count];
                for (int i = 0; i < count; i++)
                {
                    keys[i] = entries[i].Key;
                }
                return keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                TValue[] keys = new TValue[count];
                for (int i = 0; i < count; i++)
                {
                    keys[i] = entries[i].Value;
                }
                return keys;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                int i = FindEntry(key);
                if (i >= 0)
                    return entries[i].Value;
                throw new KeyNotFoundException(key.ToString());
            }
            set
            {
                Insert(key, value, false);
            }
        }

        public void Add(TKey key, TValue value)
        {
            Insert(key, value, true);
        }

        public void Clear()
        {
            lock (_syncRoot)
            {
                if (count > 0)
                {
                    for (int i = 0; i < buckets.Length; i++)
                        buckets[i] = -1;
                    count = 0;
                    OnCollectionReset();
                }
            }
        }

        public bool ContainsKey(TKey key)
        {
            return FindEntry(key) >= 0;
        }

        public bool ContainsValue(TValue value)
        {
            if (value == null)
            {
                for (int i = 0; i < count; i++)
                {
                    if (entries[i].HashCode >= 0 && entries[i].Value == null)
                        return true;
                }
            }
            else
            {
                EqualityComparer<TValue> c = EqualityComparer<TValue>.Default;
                for (int i = 0; i < count; i++)
                {
                    if (entries[i].HashCode >= 0 && c.Equals(entries[i].Value, value))
                        return true;
                }
            }
            return false;
        }

        private int FindEntry(TKey key)
        {
            if (key == null)
            {
                throw new KeyNotFoundException(nameof(key));
            }

            if (buckets != null)
            {
                int hashCode = Comparer.GetHashCode(key) & 0x7FFFFFFF;
                for (int i = buckets[hashCode % buckets.Length]; i >= 0; i = entries[i].Next)
                {
                    if (entries[i].HashCode == hashCode && Comparer.Equals(entries[i].Key, key))
                        return i;
                }
            }
            return -1;
        }

        private void Initialize(int capacity)
        {
            int size = HashHelper.GetPrime(capacity);
            buckets = new int[size];
            for (int i = 0; i < buckets.Length; i++)
                buckets[i] = -1;
            entries = new Entry[size];
        }

        private void Insert(TKey key, TValue value, bool add)
        {
            if (key == null)
            {
                throw new KeyNotFoundException(nameof(key));
            }

            lock (_syncRoot)
            {
                if (buckets == null)
                    Initialize(0);
                int hashCode = Comparer.GetHashCode(key) & 0x7FFFFFFF;
                int targetBucket = hashCode % buckets.Length;

                for (int i = buckets[targetBucket]; i >= 0; i = entries[i].Next)
                {
                    if (entries[i].HashCode == hashCode && Comparer.Equals(entries[i].Key, key))
                    {
                        if (add)
                        {
                            throw new ArgumentException("Adding Duplicate");
                        }
                        var oldValue = entries[i].Value;
                        entries[i].Value = value;
                        OnCollectionChanged(NotifyCollectionChangedAction.Replace, oldValue, value, i);
                        return;
                    }
                }
                int index;
                if (count == entries.Length)
                {
                    Resize();
                    targetBucket = hashCode % buckets.Length;
                }
                index = count;
                count++;

                entries[index].HashCode = hashCode;
                entries[index].Next = buckets[targetBucket];
                entries[index].Key = key;
                entries[index].Value = value;
                buckets[targetBucket] = index;
                OnCollectionChanged(NotifyCollectionChangedAction.Add, value, index);
            }

        }

        private void Resize()
        {
            Resize(HashHelper.ExpandPrime(count), false);
        }

        private void Resize(int newSize, bool forceNewHashCodes)
        {
            //Contract.Assert(newSize >= entries.Length);
            int[] newBuckets = new int[newSize];
            for (int i = 0; i < newBuckets.Length; i++)
                newBuckets[i] = -1;
            Entry[] newEntries = new Entry[newSize];
            Array.Copy(entries, 0, newEntries, 0, count);
            if (forceNewHashCodes)
            {
                for (int i = 0; i < count; i++)
                {
                    if (newEntries[i].HashCode != -1)
                    {
                        newEntries[i].HashCode = (Comparer.GetHashCode(newEntries[i].Key) & 0x7FFFFFFF);
                    }
                }
            }
            for (int i = 0; i < count; i++)
            {
                if (newEntries[i].HashCode >= 0)
                {
                    int bucket = newEntries[i].HashCode % newSize;
                    newEntries[i].Next = newBuckets[bucket];
                    newBuckets[bucket] = i;
                }
            }
            buckets = newBuckets;
            entries = newEntries;
        }

        public bool Remove(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (buckets != null)
            {
                int hashCode = Comparer.GetHashCode(key) & 0x7FFFFFFF;
                int bucket = hashCode % buckets.Length;
                int last = -1;
                for (int i = buckets[bucket]; i >= 0; last = i, i = entries[i].Next)
                {
                    if (entries[i].HashCode == hashCode && Comparer.Equals(entries[i].Key, key))
                    {
                        if (last < 0)
                        {
                            buckets[bucket] = entries[i].Next;
                        }
                        else
                        {
                            entries[last].Next = entries[i].Next;
                        }
                        var value = entries[i].Value;
                        entries[i].HashCode = -1;
                        entries[i].Next = -1;
                        entries[i].Key = default;
                        entries[i].Value = default;
                        OnCollectionChanged(NotifyCollectionChangedAction.Remove, value, i);
                        return true;
                    }
                }
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            int i = FindEntry(key);
            if (i >= 0)
            {
                value = entries[i].Value;
                return true;
            }
            value = default;
            return false;
        }

        void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
        {
            if(CollectionChanged != null)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, CollectionChanged, this, new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
            }
        }

        void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            if (CollectionChanged != null)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, CollectionChanged, this, new NotifyCollectionChangedEventArgs(action, item, index));
            }
        }

        void OnCollectionReset()
        {
            if (CollectionChanged != null)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, CollectionChanged, this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            return new Enumerator(this);
        }


        [Serializable]
        public struct Enumerator : IEnumerator<TValue>
        {
            private readonly ObservableHashTable<TKey, TValue> dictionary;
            private int index;
            private TValue current;

            internal Enumerator(ObservableHashTable<TKey, TValue> dictionary)
            {
                this.dictionary = dictionary;
                index = 0;
                current = default;
            }

            public bool MoveNext()
            {
                // Use unsigned comparison since we set index to dictionary.count+1 when the enumeration ends.
                // dictionary.count+1 could be negative if dictionary.count is Int32.MaxValue
                while ((uint)index < (uint)dictionary.count)
                {
                    if (dictionary.entries[index].HashCode >= 0)
                    {
                        current = dictionary.entries[index].Value;
                        index++;
                        return true;
                    }
                    index++;
                }

                index = dictionary.count + 1;
                current = default;
                return false;
            }

            public TValue Current
            {
                get { return current; }
            }

            public void Dispose()
            {
            }

            object IEnumerator.Current
            {
                get
                {
                    if (index == 0 || (index == dictionary.count + 1))
                    {
                        throw new InvalidOperationException("Operation can't happen");
                    }
                    return current;
                }
            }

            void IEnumerator.Reset()
            {
                index = 0;
                current = default;
            }
        }

        public static class HashHelper
        {
            public static readonly int[] Primes = {
            3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
            1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
            17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
            187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
            1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369};

            internal const int HashPrime = 101;

            public const int MaxPrimeArrayLength = 0x7FEFFFFD;


            public static int GetPrime(int min)
            {
                if (min < 0)
                    throw new System.ArgumentException();
                // Contract.EndContractBlock();

                for (int i = 0; i < Primes.Length; i++)
                {
                    int prime = Primes[i];
                    if (prime >= min)
                        return prime;
                }

                //outside of our predefined table. 
                //compute the hard way. 
                for (int i = (min | 1); i < int.MaxValue; i += 2)
                {
                    if (IsPrime(i) && ((i - 1) % HashPrime != 0))
                        return i;
                }
                return min;
            }

            public static bool IsPrime(int candidate)
            {
                if ((candidate & 1) != 0)
                {
                    int limit = (int)System.Math.Sqrt(candidate);
                    for (int divisor = 3; divisor <= limit; divisor += 2)
                    {
                        if ((candidate % divisor) == 0)
                            return false;
                    }
                    return true;
                }
                return (candidate == 2);
            }

            public static int ExpandPrime(int oldSize)
            {
                int newSize = 2 * oldSize;

                // Allow the hashtables to grow to maximum possible size (~2G elements) before encoutering capacity overflow.
                // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
                if ((uint)newSize > MaxPrimeArrayLength && MaxPrimeArrayLength > oldSize)
                {
                    System.Diagnostics.Contracts.Contract.Assert(MaxPrimeArrayLength == GetPrime(MaxPrimeArrayLength), "Invalid MaxPrimeArrayLength");
                    return MaxPrimeArrayLength;
                }

                return GetPrime(newSize);
            }
        }
    }
}
