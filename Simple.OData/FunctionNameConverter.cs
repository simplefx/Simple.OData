namespace Simple.OData
{
    using System.Globalization;

    class FunctionNameConverter
    {
        public string ConvertToODataName(string simpleFunctionName)
        {
            return simpleFunctionName.ToLower(CultureInfo.InvariantCulture);
        }
    }
}