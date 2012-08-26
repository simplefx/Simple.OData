using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Simple.Data.OData.Edm;

namespace Simple.Data.OData.Schema
{
    class SchemaProvider : ISchemaProvider
    {
        private string _urlBase;
        private Lazy<EdmSchema> _metadata; 

        public SchemaProvider(string urlBase)
        {
            _urlBase = urlBase;
            _metadata = new Lazy<EdmSchema>(RequestMetadata);
        }

        public IEnumerable<Table> GetTables()
        {
            return from e in _metadata.Value.EntityContainers
                   where e.IsDefaulEntityContainer
                       from s in e.EntitySets
                       select new ODataTable(s.Name, DatabaseSchema.Get(_urlBase));
        }

        public IEnumerable<Column> GetColumns(Table table)
        {
            return from e in _metadata.Value.EntityContainers
                   where e.IsDefaulEntityContainer
                       from s in e.EntitySets
                       where s.Name == table.ActualName
                           from t in _metadata.Value.EntityTypes
                           where s.EntityType.Split('.').Last() == t.Name
                               from p in t.Properties
                               select new Column(p.Name);
        }

        public IEnumerable<Association> GetAssociations(Table table)
        {
            var a = _metadata.Value.Associations.ToList();

            return from e in _metadata.Value.EntityContainers
                   where e.IsDefaulEntityContainer
                   from s in e.AssociationSets
                   where s.End.First().EntitySet == table.ActualName
                   select CreateAssociation(s.End.Last());
        }

        public Key GetPrimaryKey(Table table)
        {
            return (from e in _metadata.Value.EntityContainers
                   where e.IsDefaulEntityContainer
                       from s in e.EntitySets
                       where s.Name == table.ActualName
                           from t in _metadata.Value.EntityTypes
                           where s.EntityType.Split('.').Last() == t.Name
                           select new Key(t.Key.Properties)).Single();
        }

        private Association CreateAssociation(EdmAssociationSetEnd end)
        {
            return new Association(end.Role, end.EntitySet);
        }

        private EdmSchema RequestMetadata()
        {
            var requestBuilder = new CommandRequestBuilder(_urlBase);
            requestBuilder.AddTableCommand("$metadata", "GET");
            using (var response = new RequestRunner().TryRequest(requestBuilder.Request))
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
