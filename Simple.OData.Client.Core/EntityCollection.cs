using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public class EntityCollection
    {
        private readonly string _actualName;
        private readonly EntityCollection _baseEntityCollection;

        internal EntityCollection(string name, EntityCollection baseEntityCollection = null)
        {
            _actualName = name;
            _baseEntityCollection = baseEntityCollection;
        }

        public override string ToString()
        {
            return _actualName;
        }

        public string ActualName
        {
            get { return _actualName; }
        }

        public EntityCollection BaseEntityCollection
        {
            get { return _baseEntityCollection; }
        }
    }
}
