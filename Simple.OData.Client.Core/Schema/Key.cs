using System.Collections.Generic;
using System.Linq;

namespace Simple.OData.Client
{
    public sealed class Key
    {
        private readonly string[] _columns;

        internal Key(IEnumerable<string> columns)
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
