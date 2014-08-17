using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Simple.OData.Client.Extensions;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public class TableCollection : Collection<EntitySet>
    {
        public TableCollection()
        {
        }

        public TableCollection(IEnumerable<EntitySet> tables)
            : base(tables.ToList())
        {
        }

        public EntitySet Find(string tableName)
        {
            var table = TryFind(tableName)
                   ?? TryFind(tableName.Singularize())
                   ?? TryFind(tableName.Pluralize());

            if (table == null)
                throw new UnresolvableObjectException(tableName, string.Format("EntitySet {0} not found", tableName));

            return table;
        }

        public bool Contains(string tableName)
        {
            return TryFind(tableName) != null;
        }

        private EntitySet TryFind(string tableName)
        {
            tableName = tableName.Homogenize();
            return this.SingleOrDefault(t => t.HomogenizedName.Equals(tableName));
        }
    }
}
