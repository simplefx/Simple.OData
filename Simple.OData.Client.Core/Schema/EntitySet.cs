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
        private readonly EdmEntityType _entityType;
        private readonly EntitySet _baseEntitySet;
        private readonly Lazy<EntitySetCollection> _lazyDerivedTables;

        internal EntitySet(string name, EdmEntityType entityType, EntitySet baseEntitySet, Schema schema)
        {
            _actualName = name;
            _entityType = entityType;
            _baseEntitySet = baseEntitySet;
            _schema = schema;
            _lazyDerivedTables = new Lazy<EntitySetCollection>(GetDerivedTables);
        }

        public override string ToString()
        {
            return _actualName;
        }

        internal Schema Schema
        {
            get { return _schema; }
        }

        internal string HomogenizedName
        {
            get { return _actualName.Homogenize(); }
        }

        public string ActualName
        {
            get { return _actualName; }
        }

        public EdmEntityType EntityType
        {
            get { return _entityType; }
        }

        public EntitySet BaseEntitySet
        {
            get { return _baseEntitySet; }
        }

        public EntitySet FindDerivedTable(string tableName)
        {
            return _lazyDerivedTables.Value.Find(tableName);
        }

        public bool HasDerivedTable(string tableName)
        {
            return _lazyDerivedTables.Value.Contains(tableName);
        }

        public IDictionary<string, object> GetKey(string tableName, IDictionary<string, object> record)
        {
            var keyNames = GetKeyNames();
            return record.Where(x => keyNames.Contains(x.Key)).ToIDictionary();
        }

        public IList<string> GetKeyNames()
        {
            return _schema.ProviderMetadata.GetDeclaredKeyPropertyNames(this.ActualName).ToList();
        }

        private EntitySetCollection GetDerivedTables()
        {
            return new EntitySetCollection(_schema.Model.GetDerivedTables(this));
        }
    }
}
