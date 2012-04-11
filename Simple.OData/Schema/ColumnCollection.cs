using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Simple.Data;
using Simple.Data.Extensions;

namespace Simple.OData.Schema
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
            var column = FindColumnWithName(columnName);
            if (column == null) throw new UnresolvableObjectException(columnName);
            return column;
        }

        public bool Contains(string columnName)
        {
            return FindColumnWithName(columnName) != null;
        }

        private Column FindColumnWithName(string columnName)
        {
            columnName = columnName.Homogenize();
            return this
                .Where(c => c.HomogenizedName.Equals(columnName))
                .SingleOrDefault();
        }
    }
}
