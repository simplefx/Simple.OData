using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public abstract class ODataAdapterBase : IODataAdapter
    {
        public abstract AdapterVersion AdapterVersion { get; }
        public abstract ODataPayloadFormat DefaultPayloadFormat { get; }
        public string ProtocolVersion { get; set; }
        public object Model { get; set; }

        public abstract string GetODataVersionString();
        public abstract string ConvertValueToUriLiteral(object value);
        public abstract FunctionFormat FunctionFormat { get; }
        public abstract void FormatCommandClauses(IList<string> commandClauses, EntityCollection entityCollection,
            IList<string> expandAssociations, IList<string> selectColumns, IList<KeyValuePair<string, bool>> orderbyColumns, bool includeCount);

        public abstract IMetadata GetMetadata();
        public abstract IResponseReader GetResponseReader();
        public abstract IRequestWriter GetRequestWriter(Lazy<IBatchWriter> deferredBatchWriter);
        public abstract IBatchWriter GetBatchWriter();

        public string ConvertKeyValuesToUriLiteral(IDictionary<string, object> key, bool skipKeyNameForSingleValue)
        {
            var formattedKeyValues = key.Count == 1 && skipKeyNameForSingleValue ?
                string.Join(",", key.Select(x => ConvertValueToUriLiteral(x.Value))) :
                string.Join(",", key.Select(x => string.Format("{0}={1}", x.Key, ConvertValueToUriLiteral(x.Value))));
            return "(" + formattedKeyValues + ")";
        }

        protected string FormatExpandItem(string path, EntityCollection entityCollection)
        {
            var items = path.Split('/');
            var associationName = GetMetadata().GetNavigationPropertyExactName(entityCollection.Name, items.First());

            var text = associationName;
            if (items.Count() == 1)
            {
                return text;
            }
            else
            {
                path = path.Substring(items.First().Length + 1);

                entityCollection = GetMetadata().GetEntityCollection(
                    GetMetadata().GetNavigationPropertyPartnerName(entityCollection.Name, associationName));

                return string.Format("{0}/{1}", text, FormatExpandItem(path, entityCollection));
            }
        }

        protected string FormatSelectItem(string path, EntityCollection entityCollection)
        {
            var items = path.Split('/');
            if (items.Count() == 1)
            {
                return GetMetadata().HasStructuralProperty(entityCollection.Name, path)
                    ? GetMetadata().GetStructuralPropertyExactName(entityCollection.Name, path)
                    : GetMetadata().GetNavigationPropertyExactName(entityCollection.Name, path);
            }
            else
            {
                var associationName = GetMetadata().GetNavigationPropertyExactName(entityCollection.Name, items.First());
                var text = associationName;
                path = path.Substring(items.First().Length + 1);
                entityCollection = GetMetadata().GetEntityCollection(
                    GetMetadata().GetNavigationPropertyPartnerName(entityCollection.Name, associationName));
                return string.Format("{0}/{1}", text, FormatSelectItem(path, entityCollection));
            }
        }

        protected string FormatOrderByItem(KeyValuePair<string, bool> pathWithOrder, EntityCollection entityCollection)
        {
            var items = pathWithOrder.Key.Split('/');
            if (items.Count() == 1)
            {
                var clause = GetMetadata().HasStructuralProperty(entityCollection.Name, pathWithOrder.Key)
                    ? GetMetadata().GetStructuralPropertyExactName(entityCollection.Name, pathWithOrder.Key)
                    : GetMetadata().GetNavigationPropertyExactName(entityCollection.Name, pathWithOrder.Key);
                if (pathWithOrder.Value)
                    clause += " desc";
                return clause;
            }
            else
            {
                var associationName = GetMetadata().GetNavigationPropertyExactName(entityCollection.Name, items.First());
                var text = associationName;
                var item = pathWithOrder.Key.Substring(items.First().Length + 1);
                entityCollection = GetMetadata().GetEntityCollection(
                    GetMetadata().GetNavigationPropertyPartnerName(entityCollection.Name, associationName));
                return string.Format("{0}/{1}", text,
                    FormatOrderByItem(new KeyValuePair<string, bool>(item, pathWithOrder.Value), entityCollection));
            }
        }
    }
}