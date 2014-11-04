using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public class EntityCollection
    {
        private readonly string _name;
        private readonly EntityCollection _baseEntityCollection;

        internal EntityCollection(string name, EntityCollection baseEntityCollection = null)
        {
            _name = name;
            _baseEntityCollection = baseEntityCollection;
        }

        public override string ToString()
        {
            return _name;
        }

        public string Name
        {
            get { return _name; }
        }

        public EntityCollection BaseEntityCollection
        {
            get { return _baseEntityCollection; }
        }
    }
}
