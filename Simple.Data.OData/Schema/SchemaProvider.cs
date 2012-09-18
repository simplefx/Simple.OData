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
                   select new Table(s.Name, DatabaseSchema.Get(_urlBase));
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
            var principals = from e in _metadata.Value.EntityContainers
                             where e.IsDefaulEntityContainer
                             from s in e.AssociationSets
                             where s.End.First().EntitySet == table.ActualName
                             from a in _metadata.Value.Associations
                             where s.Association == GetQualifiedName(_metadata.Value.TypesNamespace, a.Name)
                             from n in a.End
                             where n.Role == s.End.Last().Role
                             select CreateAssociation(s.End.Last(), n);
            var dependents = from e in _metadata.Value.EntityContainers
                             where e.IsDefaulEntityContainer
                             from s in e.AssociationSets
                             where s.End.Last().EntitySet == table.ActualName
                             from a in _metadata.Value.Associations
                             where s.Association == GetQualifiedName(_metadata.Value.TypesNamespace, a.Name)
                             from n in a.End
                             where n.Role == s.End.First().Role
                             select CreateAssociation(s.End.First(), n);
            return principals.Union(dependents);
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

        private string GetQualifiedName(string schemaName, string name)
        {
            return string.IsNullOrEmpty(schemaName) ? name : string.Format("{0}.{1}", schemaName, name);
        }

        private Association CreateAssociation(EdmAssociationSetEnd associationSetEnd, EdmAssociationEnd associationEnd)
        {
            return new Association(associationSetEnd.Role, associationSetEnd.EntitySet, associationEnd.Multiplicity);
        }

        private EdmSchema RequestMetadata()
        {
            var requestBuilder = new CommandRequestBuilder(_urlBase);
            requestBuilder.AddCommand("$metadata", "GET");
            using (var response = new CommandRequestRunner(requestBuilder).TryRequest(requestBuilder.Request))
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
