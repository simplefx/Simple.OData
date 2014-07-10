using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Simple.OData.Client.Extensions;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public class TableCollection : Collection<Table>
    {
        public TableCollection()
        {
        }

        public TableCollection(IEnumerable<Table> tables)
            : base(tables.ToList())
        {
        }

        public Table Find(string tableName)
        {
            var table = TryFind(tableName)
                   ?? TryFind(tableName.Singularize())
                   ?? TryFind(tableName.Pluralize());

            if (table == null)
                throw new UnresolvableObjectException(tableName, string.Format("Table {0} not found", tableName));

            return table;
        }

        public bool Contains(string tableName)
        {
            return TryFind(tableName) != null;
        }

        private Table TryFind(string tableName)
        {
            tableName = tableName.Homogenize();
            return this.SingleOrDefault(t => t.HomogenizedName.Equals(tableName));
        }
    }
}
