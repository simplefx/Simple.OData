using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.OData.Schema
{
    public sealed class Key
    {
        private readonly string[] _columns;

        public Key(IEnumerable<string> columns)
        {
            _columns = columns.ToArray();
        }

        public string this[int index]
        {
            get { return _columns[index]; }
        }

        public IEnumerable<string> AsEnumerable()
        {
            return _columns.AsEnumerable();
        }
    }
}
