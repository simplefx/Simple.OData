using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.OData.Client
{
    class SchemaProvider
    {
        private readonly Schema _schema;

        internal SchemaProvider(Schema schema)
        {
            _schema = schema;
        }

        public string GetTypesNamespace()
        {
            return _schema.Metadata.TypesNamespace;
        }

        public string GetContainersNamespace()
        {
            return _schema.Metadata.ContainersNamespace;
        }

        public IEnumerable<Table> GetTables()
        {
            return from s in GetEntitySets()
                   from et in GetEntitySetType(s)
                   select new Table(s.Name, et, null, _schema);
        }

        public IEnumerable<Table> GetDerivedTables(Table table)
        {
            return from et in _schema.Metadata.EntityTypes
                   where et.BaseType != null && et.BaseType.Name == table.EntityType.Name
                   select new Table(et.Name, et, table, _schema);
        }

        public IEnumerable<Column> GetColumns(Table table)
        {
            return from t in GetEntityTypeWithBaseTypes(table.EntityType)
                   from p in t.Properties
                   select new Column(p.Name, p.Type, p.Nullable);
        }

        public IEnumerable<Association> GetAssociations(Table table)
        {
            var principals = from e in _schema.Metadata.EntityContainers
                             //where e.IsDefaulEntityContainer
                             from s in e.AssociationSets
                             where s.End.First().EntitySet == table.ActualName
                             from a in _schema.Metadata.Associations
                             where s.Association == GetQualifiedName(_schema.Metadata.TypesNamespace, a.Name)
                             from n in a.End
                             where n.Role == s.End.Last().Role
                             from t in GetEntityTypeWithBaseTypes(table.EntityType)
                             from np in t.NavigationProperties
                             where np.Relationship == GetQualifiedName(_schema.Metadata.TypesNamespace, a.Name) && np.ToRole == n.Role
                             select CreateAssociation(np.Name, s.End.Last(), n);
            var dependents = from e in _schema.Metadata.EntityContainers
                             //where e.IsDefaulEntityContainer
                             from s in e.AssociationSets
                             where s.End.Last().EntitySet == table.ActualName
                             from a in _schema.Metadata.Associations
                             where s.Association == GetQualifiedName(_schema.Metadata.TypesNamespace, a.Name)
                             from n in a.End
                             where n.Role == s.End.First().Role
                             from t in GetEntityTypeWithBaseTypes(table.EntityType)
                             from np in t.NavigationProperties
                             where np.Relationship == GetQualifiedName(_schema.Metadata.TypesNamespace, a.Name) && np.ToRole == n.Role
                             select CreateAssociation(np.Name, s.End.First(), n);
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
            return from e in _schema.Metadata.EntityContainers
                   //where e.IsDefaulEntityContainer
                   from f in e.FunctionImports
                   select CreateFunction(f);
        }

        public IEnumerable<EdmEntityType> GetEntityTypes()
        {
            return from t in _schema.Metadata.EntityTypes
                   select t;
        }

        public IEnumerable<EdmComplexType> GetComplexTypes()
        {
            return from t in _schema.Metadata.ComplexTypes
                   select t;
        }

        private IEnumerable<EdmEntitySet> GetEntitySets()
        {
            return from e in _schema.Metadata.EntityContainers
                   //where e.IsDefaulEntityContainer
                   from s in e.EntitySets
                   select s;
        }

        private IEnumerable<EdmEntityType> GetEntitySetType(EdmEntitySet entitySet)
        {
            return from et in _schema.Metadata.EntityTypes
                   where entitySet.EntityType.Split('.').Last() == et.Name
                   select et;
        }

        private string GetQualifiedName(string schemaName, string name)
        {
            return string.IsNullOrEmpty(schemaName) ? name : string.Format("{0}.{1}", schemaName, name);
        }

        private Association CreateAssociation(string associationName, EdmAssociationSetEnd associationSetEnd, EdmAssociationEnd associationEnd)
        {
            return new Association(associationName, associationSetEnd.EntitySet, associationEnd.Multiplicity);
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
    }
}
