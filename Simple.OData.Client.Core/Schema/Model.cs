using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.OData.Client
{
    class Model
    {
        private readonly Schema _schema;

        internal Model(Schema schema)
        {
            _schema = schema;
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

        public Key GetPrimaryKey(Table table)
        {
            return (from s in GetEntitySets()
                    where s.Name == table.ActualName
                    from et in GetEntitySetType(s)
                    from t in GetEntityTypeWithBaseTypes(et)
                    where t.Key != null
                    select new Key(t.Key.Properties)).SingleOrDefault();
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
