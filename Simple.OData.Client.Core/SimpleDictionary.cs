#if NET40
using System.Collections.Concurrent;
using System.Collections.Generic;
#else
using System;
using System.Collections.Generic;
#endif

namespace Simple.OData.Client
{
    internal class SimpleDictionary<TKey, TValue> :
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

#if !NET40
        public TValue GetOrAdd(TKey key, TValue value)
        {
            TValue storedValue;
            if (base.TryGetValue(key, out storedValue))
                value = storedValue;
            else
                base.Add(key, value);
            return value;
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            TValue value;
            if (!base.TryGetValue(key, out value))
            {
                value = valueFactory(key);
                base.Add(key, value);
            }
            return value;
        }
#endif
    }
}
