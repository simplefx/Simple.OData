using System;
using System.Collections.Generic;
using System.Linq;
using Simple.OData.Client.Extensions;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public class EntitySet
    {
        private readonly Schema _schema;
        private readonly string _actualName;
        //private readonly EdmEntityType _entityType;
        private readonly EntitySet _baseEntitySet;

        internal EntitySet(string name, EntitySet baseEntitySet, Schema schema)
//        internal EntitySet(string name, EdmEntityType entityType, EntitySet baseEntitySet, Schema schema)
        {
            _actualName = name;
            //_entityType = entityType;
            _baseEntitySet = baseEntitySet;
            _schema = schema;
        }

        public override string ToString()
        {
            return _actualName;
        }

        internal Schema Schema
        {
            get { return _schema; }
        }

        public string ActualName
        {
            get { return _actualName; }
        }

        //public EdmEntityType EntityType
        //{
        //    get { return _entityType; }
        //}

        public EntitySet BaseEntitySet
        {
            get { return _baseEntitySet; }
        }

        public EntitySet FindDerivedEntitySet(string entityTypeName)
        {
            throw new NotImplementedException();
        }

        public bool HasDerivedEntitySet(string entityTypeName)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, object> GetKey(string entityTypeName, IDictionary<string, object> record)
        {
            var keyNames = GetKeyNames();
            return record.Where(x => keyNames.Contains(x.Key)).ToIDictionary();
        }

        public IList<string> GetKeyNames()
        {
            return _schema.ProviderMetadata.GetDeclaredKeyPropertyNames(this.ActualName).ToList();
        }

        private EntitySetCollection GetDerivedEntitySets()
        {
            throw new NotImplementedException();
        }
    }
}
