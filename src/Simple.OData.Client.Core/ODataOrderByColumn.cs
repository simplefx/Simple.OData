using System;

namespace Simple.OData.Client
{
    public class ODataOrderByColumn
    {
        public string Name { get; }
        public bool Descending { get; }

        public ODataOrderByColumn(string name, bool descending)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"Parameter {nameof(name)} should not be null or empty.", nameof(name));
            Name = name;
            Descending = descending;
        }
    }
}