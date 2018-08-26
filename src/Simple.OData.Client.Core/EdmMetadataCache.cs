using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    class EdmMetadataCache
    {
        static readonly object metadataLock = new object();
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
            lock (metadataLock)
            {
                if (!_instances.TryGetValue(key, out var found))
                {
                    _instances[key] = found = valueFactory(key);
                }
                return found;
            }
        }

        public static async Task<EdmMetadataCache> GetOrAddAsync(string key, Func<string, Task<EdmMetadataCache>> valueFactory)
        {
            EdmMetadataCache found;
            lock (metadataLock)
            {
                if (_instances.TryGetValue(key, out found))
                    return found;
            }
            found = await valueFactory(key).ConfigureAwait(false);
            lock(metadataLock)
            {
                if (!_instances.ContainsKey(key))
                    _instances[key] = found;
                return _instances[key];
            }
        }

        private readonly Func<ISession, IODataAdapter> _adapterFactory;

        public EdmMetadataCache(string key, string metadataDocument)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrWhiteSpace(metadataDocument))
                throw new ArgumentNullException(nameof(metadataDocument));

            Key = key;
            MetadataDocument = metadataDocument;
            _adapterFactory = new AdapterFactory().CreateAdapter(metadataDocument);
        }

        public string Key { get; }

        public string MetadataDocument { get; }

        public IODataAdapter GetODataAdapter(ISession session)
        {
            return _adapterFactory(session);
        }
    }
}
