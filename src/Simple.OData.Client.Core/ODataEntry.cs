using System.Collections.Generic;

namespace Simple.OData.Client
{
    /// <summary>
    /// A container for an OData entry properties. Normally not used directly by the client code.
    /// </summary>
    public class ODataEntry
    {
        /// <summary>
        /// The content of the OData entry.
        /// </summary>
        protected Dictionary<string, object> _entry;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEntry"/> class.
        /// </summary>
        public ODataEntry()
        {
            _entry = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEntry"/> class.
        /// </summary>
        /// <param name="entry">The collection of entry properties.</param>
        public ODataEntry(IDictionary<string, object> entry)
        {
            _entry = new Dictionary<string, object>(entry);
        }

        /// <summary>
        /// Gets or sets the value of the specified property.
        /// </summary>
        /// <value>
        /// The property value.
        /// </value>
        /// <param name="key">The property name.</param>
        /// <returns>The property value.</returns>
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

        /// <summary>
        /// Returns OData entry properties as dictionary.
        /// </summary>
        /// <returns>A dictionary of OData entry properties.</returns>
        public IDictionary<string, object> AsDictionary()
        {
            return _entry;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="T:System.Collections.Generic.Dictionary{System.String,System.Object}"/> to <see cref="ODataEntry"/>.
        /// </summary>
        /// <param name="entry">The property collection.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator ODataEntry(Dictionary<string, object> entry)
        {
            return new ODataEntry() { _entry = entry };
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="ODataEntry"/> to <see cref="T:System.Collections.Generic.Dictionary{System.String,System.Object}"/>.
        /// </summary>
        /// <param name="entry">The OData entry.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator Dictionary<string, object>(ODataEntry entry)
        {
            return entry._entry;
        }
    }
}
