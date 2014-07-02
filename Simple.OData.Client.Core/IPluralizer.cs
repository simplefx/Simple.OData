namespace Simple.OData.Client
{
    public interface IPluralizer
    {
        string Pluralize(string word);
        string Singularize(string word);
    }
}
