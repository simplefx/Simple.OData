using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    class EdmMetadataCache
    {
        static readonly object metadataLock = new object();
        // TODO: Do we want to swap for ConcurrentDictionary?
        static readonly IDictionary<string, EdmMetadataCache> _instances = new Dictionary<string, EdmMetadataCache>();

        public static void Clear()
        {
            lock (metadataLock)
            {
                _instances.Clear();
                DictionaryExtensions.ClearCache();
            }
        }

        public static void Clear(string key)
        {
            lock (metadataLock)
            {
                _instances.Remove(key);
            }
        }

        public static EdmMetadataCache GetOrAdd(string key, Func<string, EdmMetadataCache> valueFactory)
        {
            // Double lock check, cheaper to check outside the lock first
            // ReSharper disable once InconsistentlySynchronizedField
            if (_instances.TryGetValue(key, out var found))
            {
                return found;
            }

            // Now get it, don't lock as might be expensive
            found = valueFactory(key);

            // Check again and update cache
            lock (metadataLock)
            {
                if (!_instances.ContainsKey(key))
                {
                    _instances[key] = found;
                }

                return _instances[key];
            }
        }

        public static async Task<EdmMetadataCache> GetOrAddAsync(string key, Func<string, Task<EdmMetadataCache>> valueFactory)
        {
            // Double lock check, cheaper to check outside the lock first
            // ReSharper disable once InconsistentlySynchronizedField
            if (_instances.TryGetValue(key, out var found))
            {
                return found;
            }

            // Now get it, don't lock as might be expensive
            found = await valueFactory(key).ConfigureAwait(false);

            // Check again and update cache
            lock (metadataLock)
            {
                if (!_instances.ContainsKey(key))
                {
                    _instances[key] = found;
                }

                return _instances[key];
            }
        }

        private readonly Func<ISession, IODataAdapter> _adapterFactory;

        public EdmMetadataCache(string key, string metadataDocument, ITypeCache typeCache)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrWhiteSpace(metadataDocument))
                throw new ArgumentNullException(nameof(metadataDocument));

            Key = key;
            MetadataDocument = metadataDocument;
            TypeCache = typeCache;
            _adapterFactory = new AdapterFactory().CreateAdapter(metadataDocument);
        }

        public string Key { get; }

        public string MetadataDocument { get; }

        public ITypeCache TypeCache { get; }

        public IODataAdapter GetODataAdapter(ISession session)
        {
            return _adapterFactory(session);
        }
    }
}
