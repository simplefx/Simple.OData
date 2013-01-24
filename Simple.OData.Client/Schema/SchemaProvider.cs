using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Simple.OData.Client
{
    class SchemaProvider
    {
        private readonly Lazy<EdmSchema> _metadata;
        private readonly Lazy<string> _metadataString;
        private readonly Lazy<Schema> _schema;

        private SchemaProvider(string urlBase, Credentials credentials, string metadataString)
        {
            if (!string.IsNullOrEmpty(metadataString))
            {
                _metadataString = new Lazy<string>(() => metadataString);
            }
            else
            {
                _metadataString = new Lazy<string>(() => RequestMetadataAsString(urlBase, credentials));
            }
            _metadata = new Lazy<EdmSchema>(() => ODataFeedReader.GetSchema(_metadataString.Value));
            _schema = new Lazy<Schema>(() => Schema.Get(this));
        }

        public static SchemaProvider FromUrl(string urlBase, Credentials credentials)
        {
            return new SchemaProvider(urlBase, credentials, null);
        }

        public static SchemaProvider FromMetadata(string metadataString)
        {
            return new SchemaProvider(null, null, metadataString);
        }

        public Schema Schema
        {
            get { return _schema.Value; }
        }

        public string SchemaAsString
        {
            get { return _metadataString.Value; }
        }

        public string GetTypesNamespace()
        {
            return _metadata.Value.TypesNamespace;
        }

        public string GetContainersNamespace()
        {
            return _metadata.Value.ContainersNamespace;
        }

        public IEnumerable<Table> GetTables()
        {
            return from s in GetEntitySets()
                   from et in GetEntitySetType(s)
                   select new Table(s.Name, et, null, _schema.Value);
        }

        public IEnumerable<Table> GetDerivedTables(Table table)
        {
            return from et in _metadata.Value.EntityTypes
                   where et.BaseType != null && et.BaseType.Name == table.EntityType.Name 
                   select new Table(et.Name, et, table, _schema.Value);
        }

        public IEnumerable<Column> GetColumns(Table table)
        {
            return from t in GetEntityTypeWithBaseTypes(table.EntityType)
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
            return (from s in GetEntitySets()
                    where s.Name == table.ActualName
                    from et in GetEntitySetType(s)
                    from t in GetEntityTypeWithBaseTypes(et)
                    where t.Key != null
                    select new Key(t.Key.Properties)).SingleOrDefault();
        }

        public IEnumerable<Function> GetFunctions()
        {
            return from e in _metadata.Value.EntityContainers
                   where e.IsDefaulEntityContainer
                   from f in e.FunctionImports
                   select CreateFunction(f);
        }

        public IEnumerable<EdmEntityType> GetEntityTypes()
        {
            return from t in _metadata.Value.EntityTypes
                   select t;
        }

        public IEnumerable<EdmComplexType> GetComplexTypes()
        {
            return from t in _metadata.Value.ComplexTypes
                   select t;
        }

        private IEnumerable<EdmEntitySet> GetEntitySets()
        {
            return from e in _metadata.Value.EntityContainers
                   where e.IsDefaulEntityContainer
                   from s in e.EntitySets
                   select s;
        }

        private IEnumerable<EdmEntityType> GetEntitySetType(EdmEntitySet entitySet)
        {
            return from et in _metadata.Value.EntityTypes
                   where entitySet.EntityType.Split('.').Last() == et.Name
                   select et;
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

        private IEnumerable<EdmEntityType> GetEntityTypeWithBaseTypes(EdmEntityType entityType)
        {
            if (entityType.BaseType == null)
            {
                yield return entityType;
            }
            else
            {
                var baseTypes = GetEntityTypeWithBaseTypes(entityType.BaseType);
                foreach (var baseType in baseTypes)
                {
                    yield return baseType;
                }
                yield return entityType;
            }
        }

        private string RequestMetadataAsString(string urlBase, Credentials credentials)
        {
            var requestBuilder = new CommandRequestBuilder(urlBase, credentials);
            var command = HttpCommand.Get(ODataCommand.MetadataLiteral);
            requestBuilder.AddCommandToRequest(command);
            using (var response = new CommandRequestRunner().TryRequest(command.Request))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return ODataFeedReader.GetSchemaAsString(response.GetResponseStream());
                }
            }
            // TODO
            return null;
        }
    }
}
