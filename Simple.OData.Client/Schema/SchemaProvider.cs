using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Simple.OData.Client
{
    class SchemaProvider : ISchemaProvider
    {
        private Lazy<EdmSchema> _metadata;
        private Lazy<string> _metadataString;
        private Lazy<Schema> _schema; 

        private SchemaProvider(string urlBase, string metadataString)
        {
            if (!string.IsNullOrEmpty(metadataString))
            {
                _metadataString = new Lazy<string>(() => metadataString);
            }
            else
            {
                _metadataString = new Lazy<string>(() => RequestMetadataAsString(urlBase));
            }
            _metadata = new Lazy<EdmSchema>(() => ODataHelper.GetSchema(_metadataString.Value));
            _schema = new Lazy<Schema>(() => Schema.Get(this));
        }

        public static SchemaProvider FromUrl(string urlBase)
        {
            return new SchemaProvider(urlBase,null);
        }

        public static SchemaProvider FromMetadata(string metadataString)
        {
            return new SchemaProvider(null, metadataString);
        }

        public Schema Schema
        {
            get { return _schema.Value; }
        }

        public string SchemaAsString
        {
            get { return _metadataString.Value; }
        }

        public IEnumerable<Table> GetTables()
        {
            return from e in _metadata.Value.EntityContainers
                   where e.IsDefaulEntityContainer
                   from s in e.EntitySets
                   select new Table(s.Name, _schema.Value);
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
                   select new Column(p.Name, p.Type, p.Nullable);
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

        public IEnumerable<Function> GetFunctions()
        {
            return from e in _metadata.Value.EntityContainers
                   where e.IsDefaulEntityContainer
                   from f in e.FunctionImports
                   select CreateFunction(f);
        }

        private string GetQualifiedName(string schemaName, string name)
        {
            return string.IsNullOrEmpty(schemaName) ? name : string.Format("{0}.{1}", schemaName, name);
        }

        private Association CreateAssociation(EdmAssociationSetEnd associationSetEnd, EdmAssociationEnd associationEnd)
        {
            return new Association(associationSetEnd.Role, associationSetEnd.EntitySet, associationEnd.Multiplicity);
        }

        private Function CreateFunction(EdmFunctionImport f)
        {
            return new Function(
                f.Name, 
                f.HttpMethod, 
                f.EntitySet, 
                f.ReturnType == null ? null : f.ReturnType.Name,
                f.Parameters.Select(p => p.Name));
        }

        private string RequestMetadataAsString(string urlBase)
        {
            var requestBuilder = new CommandRequestBuilder(urlBase);
            var command = HttpCommand.Get(ODataCommand.MetadataLiteral);
            requestBuilder.AddCommandToRequest(command);
            using (var response = new CommandRequestRunner().TryRequest(command.Request))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return ODataHelper.GetSchemaAsString(response.GetResponseStream());
                }
            }
            // TODO
            return null;
        }
    }
}
