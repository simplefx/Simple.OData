using System.Collections.Concurrent;

namespace Simple.OData.Client.Extensions
{
    class TypeCaches
    {
        static readonly ConcurrentDictionary<string, ITypeCache> _typeCaches = new ConcurrentDictionary<string, ITypeCache>();

        public static ITypeCache GetOrAdd(string key)
        {
            return _typeCaches.GetOrAdd(key, x => new TypeCache());
        }
    }
}
