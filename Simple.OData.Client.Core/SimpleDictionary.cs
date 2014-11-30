#if NET40
using System.Collections.Concurrent;
using System.Collections.Generic;
#else
using System;
using System.Collections.Generic;
#endif

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public class SimpleDictionary<TKey, TValue> :
#if NET40
        ConcurrentDictionary<TKey, TValue>
#else
        Dictionary<TKey, TValue>
#endif
    {
        public SimpleDictionary()
        {
        }

        public SimpleDictionary(IEqualityComparer<TKey> comparer)
            : base(comparer)
        {
        }

#if NET40
        public void Remove(TKey key)
        {
            TValue value;
            base.TryRemove(key, out value);
        }
#else
        public TValue GetOrAdd(TKey key, TValue value)
        {
            return GetOrAdd(key, x => value);
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            TValue value;
            TValue storedValue;
            if (base.TryGetValue(key, out storedValue))
            {
                value = storedValue;
            }
            else
            {
                lock (this)
                {
                    if (base.TryGetValue(key, out storedValue))
                    {
                        value = storedValue;
                    }
                    else
                    {
                        value = valueFactory(key);
                        base.Add(key, value);
                    }
                }
            }
            return value;
        }
#endif
    }
}
