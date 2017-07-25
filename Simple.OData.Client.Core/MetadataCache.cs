using System;
#if NET40
using System.Collections.Concurrent;
#endif
using System.Collections.Generic;
using System.Threading.Tasks;

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

        private string _metadataDocument;
        private Task _resolutionTask;

        public MetadataCache(string key)
        {
            if (String.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");

            _key = key;
        }

        public MetadataCache(string key, string metadataDocument)
        {
            if (String.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");
            if (String.IsNullOrWhiteSpace(metadataDocument))
                throw new ArgumentNullException("metadataDocument");

            _key = key;
            SetMetadataDocument(metadataDocument);

#if !NET40
            _resolutionTask = Task.FromResult(metadataDocument);
#else
            var tcs = new TaskCompletionSource<string>();
            tcs.SetResult(metadataDocument);
            _resolutionTask = tcs.Task;
#endif
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
                if (_metadataDocument == null)
                    throw new InvalidOperationException("Service metadata is not resolved");

                return _metadataDocument;
            }
        }

        public Task Resolved
        {
            get
            {
                return _resolutionTask;
            }
        }

        public void SetMetadataDocument(Task<string> metadataResolution)
        {
            _resolutionTask = metadataResolution.ContinueWith(t => SetMetadataDocument(t.Result));
        }

        public void SetMetadataDocument(string metadataString)
        {
            _metadataDocument = metadataString;
            _adapterFactory = new AdapterFactory().CreateAdapter(metadataString);
        }

        public IODataAdapter GetODataAdapter(ISession session)
        {
            return _adapterFactory(session);
        }
    }
}
