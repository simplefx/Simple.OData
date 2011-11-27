using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Simple.Data.OData.Helpers;
using Simple.Data.OData.Schema;
using Simple.OData;
using Simple.OData.Edm;

namespace Simple.Data.OData
{
    class SchemaProvider : ISchemaProvider
    {
        private ProviderHelper _providerHelper;
        private Lazy<EdmSchema> _metadata; 

        public SchemaProvider(ProviderHelper providerHelper)
        {
            _providerHelper = providerHelper;
            _metadata = new Lazy<EdmSchema>(RequestMetadata);
        }

        public IEnumerable<Table> GetTables()
        {
            return from e in _metadata.Value.EntityContainers
                   where e.IsDefaulEntityContainer
                       from s in e.EntitySets
                       select new Table(s.Name, _providerHelper);
        }

        public IEnumerable<Column> GetColumns(Table table)
        {
            return from e in _metadata.Value.EntityContainers
                   where e.IsDefaulEntityContainer
                       from s in e.EntitySets
                       where s.Name == table.ActualName
                           from c in s.EntityType
                           select new Column(s.Name, table);
        }

        public Key GetPrimaryKey(Table table)
        {
            return (from e in _metadata.Value.EntityContainers
                   where e.IsDefaulEntityContainer
                       from s in e.EntitySets
                       where s.Name == table.ActualName
                           from t in _metadata.Value.EntityTypes
                           where s.Name == t.Name
                           select new Key(t.Key.Properties)).Single();
        }

        private EdmSchema RequestMetadata()
        {
            var request = _providerHelper.CreateTableRequest("$metadata", "GET");
            using (var response = new RequestRunner().TryRequest(request))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return DataServicesHelper.GetSchema(response.GetResponseStream());
                }
            }
            // TODO
            return null;
        }
    }
}
