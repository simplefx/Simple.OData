using System;
#if NET40
using System.Collections.Concurrent;
#else
using System.Collections.Generic;
#endif

namespace Simple.OData.Client
{
    class MetadataCache
    {
#if NET40
        static readonly ConcurrentDictionary<string, MetadataCache> _instances = new ConcurrentDictionary<string, MetadataCache>();
#else
        static readonly object metadataLock = new object();
        static readonly IDictionary<string, MetadataCache> _instances = new Dictionary<string, MetadataCache>();
#endif

        public static void Clear()
        {
#if NET40
            _instances.Clear();
#else
            lock (metadataLock)
            {
                _instances.Clear();
            }
#endif
        }

        public static void Clear(string key)
        {
#if NET40
            MetadataCache _ignored;
            _instances.TryRemove(key, out _ignored);
#else
            lock (metadataLock)
            {
                _instances.Remove(key);
            }
#endif
        }

        public static MetadataCache GetOrAdd(string key, Func<string, MetadataCache> valueFactory)
        {
#if NET40
            return _instances.GetOrAdd(key, valueFactory);
#else
            lock (metadataLock)
            {
                MetadataCache found;
                if (!_instances.TryGetValue(key, out found))
                {
                    _instances[key] = found = valueFactory(key);
                }

                return found;
            }
#endif
        }

        private readonly string _key;
        private Func<ISession, IODataAdapter> _adapterFactory;

        private readonly string _metadataDocument;
        
        public MetadataCache(string key, string metadataDocument)
        {
            if (String.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");
            if (String.IsNullOrWhiteSpace(metadataDocument))
                throw new ArgumentNullException("metadataDocument");

            _key = key;
            _metadataDocument = metadataDocument;
            _adapterFactory = new AdapterFactory().CreateAdapter(metadataDocument);
        }

        public string Key
        {
            get
            {
                return _key;
            }
        }

        public string MetadataDocument
        {
            get
            {
                return _metadataDocument;
            }
        }

        public IODataAdapter GetODataAdapter(ISession session)
        {
            return _adapterFactory(session);
        }
    }
}
