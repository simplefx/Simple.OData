using System.Collections.Generic;

namespace Simple.OData.Client
{
    public class ODataEntry
    {
        protected Dictionary<string, object> _entry;

        public ODataEntry()
        {
            _entry = new Dictionary<string, object>();
        }

        public ODataEntry(IDictionary<string, object> entry)
        {
            _entry = new Dictionary<string, object>(entry);
        }

        public object this[string key]
        {
            get
            {
                return _entry[key];
            }
            set
            {
                if (_entry.ContainsKey(key))
                    _entry[key] = value;
                else
                    _entry.Add(key, value);
            }
        }

        public IDictionary<string, object> AsDictionary()
        {
            return _entry;
        }

        public static explicit operator ODataEntry(Dictionary<string, object> entry)
        {
            return new ODataEntry() { _entry = entry };
        }

        public static explicit operator Dictionary<string, object>(ODataEntry entry)
        {
            return entry._entry;
        }
    }
}
