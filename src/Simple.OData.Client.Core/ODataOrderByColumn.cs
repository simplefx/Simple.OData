namespace Simple.OData.Client
{
    public class ODataOrderByColumn
    {
        public string Name { get; }
        public bool Descending { get; }

        public ODataOrderByColumn(string name, bool descending)
        {
            Name = name;
            Descending = @descending;
        }
    }
}