using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            var actualName = _schema.ProviderMetadata.GetDerivedEntityTypeExactName(this.ActualName, entityTypeName);
            return new EntitySet(actualName, this, _schema);
        }

        public bool HasDerivedEntitySet(string entityTypeName)
        {
            return _schema.ProviderMetadata.GetDerivedEntityTypeNames(this.ActualName)
                .Any(x => ProviderMetadata.NamesAreEqual(x, entityTypeName));
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
    }
}
