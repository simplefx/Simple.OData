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
        private readonly Session _session;
        private readonly string _actualName;
        //private readonly EdmEntityType _entityType;
        private readonly EntitySet _baseEntitySet;

        internal EntitySet(string name, EntitySet baseEntitySet, Session session)
//        internal EntitySet(string name, EdmEntityType entityType, EntitySet baseEntitySet, Session Session)
        {
            _actualName = name;
            //_entityType = entityType;
            _baseEntitySet = baseEntitySet;
            _session = session;
        }

        public override string ToString()
        {
            return _actualName;
        }

        internal Session Session
        {
            get { return _session; }
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
            var actualName = _session.Provider.GetMetadata().GetDerivedEntityTypeExactName(this.ActualName, entityTypeName);
            return new EntitySet(actualName, this, _session);
        }

        public bool HasDerivedEntitySet(string entityTypeName)
        {
            return _session.Provider.GetMetadata().GetDerivedEntityTypeNames(this.ActualName)
                .Any(x => Utils.NamesAreEqual(x, entityTypeName));
        }

        public IDictionary<string, object> GetKey(string entityTypeName, IDictionary<string, object> record)
        {
            var keyNames = GetKeyNames();
            return record.Where(x => keyNames.Contains(x.Key)).ToIDictionary();
        }

        public IList<string> GetKeyNames()
        {
            return _session.Provider.GetMetadata().GetDeclaredKeyPropertyNames(this.ActualName).ToList();
        }
    }
}
