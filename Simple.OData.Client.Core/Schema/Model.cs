using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.OData.Client
{
    //class Model
    //{
    //    private readonly Schema _schema;

    //    internal Model(Schema schema)
    //    {
    //        _schema = schema;
    //    }

    //    public IEnumerable<EntitySet> GetTables()
    //    {
    //        return from s in GetEntitySets()
    //               from et in GetEntitySetType(s)
    //               select new EntitySet(s.Name, et, null, _schema);
    //    }

    //    public IEnumerable<EntitySet> GetDerivedTables(EntitySet entitySet)
    //    {
    //        return from et in _schema.Metadata.EntityTypes
    //               where et.BaseType != null && et.BaseType.Name == entitySet.EntityType.Name
    //               select new EntitySet(et.Name, et, entitySet, _schema);
    //    }

    //    public IEnumerable<EdmEntityType> GetEntityTypes()
    //    {
    //        return from t in _schema.Metadata.EntityTypes
    //               select t;
    //    }

    //    public IEnumerable<EdmComplexType> GetComplexTypes()
    //    {
    //        return from t in _schema.Metadata.ComplexTypes
    //               select t;
    //    }

    //    private IEnumerable<EdmEntitySet> GetEntitySets()
    //    {
    //        return from e in _schema.Metadata.EntityContainers
    //               //where e.IsDefaulEntityContainer
    //               from s in e.EntitySets
    //               select s;
    //    }

    //    private IEnumerable<EdmEntityType> GetEntitySetType(EdmEntitySet entitySet)
    //    {
    //        return from et in _schema.Metadata.EntityTypes
    //               where entitySet.EntityType.Split('.').Last() == et.Name
    //               select et;
    //    }
    //}
}
