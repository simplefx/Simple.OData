using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Concurrent
{
    public class ConcurrentDictionary<TKey, TValue> : 
        IDictionary<TKey, TValue>,
        ICollection<KeyValuePair<TKey, TValue>>, 
        IEnumerable<KeyValuePair<TKey, TValue>>,
        IDictionary, ICollection, IEnumerable
    {
        private Dictionary<TKey, TValue> _dictionary; 

        public ConcurrentDictionary()
        {
            _dictionary = new Dictionary<TKey, TValue>();
        }

        public ConcurrentDictionary(IEqualityComparer<TKey> comparer)
        {
            _dictionary = new Dictionary<TKey, TValue>(comparer);
        }

        public ConcurrentDictionary(int concurrencyLevel, int capacity)
        {
            _dictionary = new Dictionary<TKey, TValue>(capacity);
        }

        public ConcurrentDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer)
        {
            _dictionary = new Dictionary<TKey, TValue>(capacity, comparer);
        }

        public bool Contains(object key)
        {
            return _dictionary.ContainsKey((TKey)key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        public void Remove(object key)
        {
            _dictionary.Remove((TKey)key);
        }

        public bool IsFixedSize { get; private set; }
        public bool IsReadOnly { get; private set; }

        object IDictionary.this[object key]
        {
            get { return _dictionary[(TKey)key]; }
            set { _dictionary[(TKey)key] = (TValue)value; }
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _dictionary.Add(item.Key, item.Value);
        }

        public void Add(object key, object value)
        {
            _dictionary.Add((TKey)key, (TValue)value);
        }

        void IDictionary.Clear()
        {
            _dictionary.Clear();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            _dictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return this.Contains(item) && _dictionary.Remove(item.Key);
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public int Count { get; set; }
        public bool IsSynchronized { get; private set; }
        public object SyncRoot { get; private set; }

        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            return _dictionary.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get { return _dictionary[key]; }
            set { _dictionary[key] = value; }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get { return _dictionary.Keys; }
        }

        ICollection IDictionary.Values
        {
            get { return _dictionary.Values; }
        }

        ICollection IDictionary.Keys
        {
            get { return _dictionary.Keys; }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get { return _dictionary.Values; }
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            throw new NotImplementedException();
        }

        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            throw new NotImplementedException();
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            throw new NotImplementedException();
        }
        public TValue GetOrAdd (TKey key, TValue value)
        {
            throw new NotImplementedException();
        }
    }
}
