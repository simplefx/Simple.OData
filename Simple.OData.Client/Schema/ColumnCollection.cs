using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Simple.OData.Client
{
    public class ColumnCollection : Collection<Column>
    {
        public ColumnCollection()
        {
        }

        public ColumnCollection(IEnumerable<Column> columns)
            : base(columns.ToList())
        {
        }

        public Column Find(string columnName)
        {
            var column = TryFind(columnName);
            if (column == null) throw new UnresolvableObjectException(columnName, string.Format("Column {0} not found", columnName));
            return column;
        }

        public bool Contains(string columnName)
        {
            return TryFind(columnName) != null;
        }

        private Column TryFind(string columnName)
        {
            columnName = columnName.Homogenize();
            return this
                .Where(c => c.HomogenizedName.Equals(columnName))
                .SingleOrDefault();
        }
    }
}
