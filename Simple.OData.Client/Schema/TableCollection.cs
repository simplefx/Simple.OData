using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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

        /// <summary>
        /// Finds the Table with a name most closely matching the specified table name.
        /// This method will try an exact match first, then a case-insensitve search, then a pluralized or singular version.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>A <see cref="Table"/> if a match is found; otherwise, <c>null</c>.</returns>
        public Table Find(string tableName)
        {
            var table = TryFind(tableName)
                   ?? FindTableWithPluralName(tableName)
                   ?? FindTableWithSingularName(tableName);

            if (table == null)
            {
                throw new UnresolvableObjectException(tableName, string.Format("Table {0} not found", tableName));
            }

            return table;
        }

        private Table FindTableWithSingularName(string tableName)
        {
            return TryFind(tableName.Singularize());
        }

        private Table FindTableWithPluralName(string tableName)
        {
            return TryFind(tableName.Pluralize());
        }

        public bool Contains(string tableName)
        {
            return TryFind(tableName) != null;
        }

        private Table TryFind(string tableName)
        {
            tableName = tableName.Homogenize();
            return this
                .Where(t => t.HomogenizedName.Equals(tableName))
                .SingleOrDefault();
        }
    }
}
