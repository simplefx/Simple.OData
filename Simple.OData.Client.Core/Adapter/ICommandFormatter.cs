using System.Collections.Generic;

namespace Simple.OData.Client
{
    public interface ICommandFormatter
    {
        string FormatCommand(FluentCommand command);
        string ConvertKeyValuesToUriLiteral(IDictionary<string, object> key, bool skipKeyNameForSingleValue);
        string ConvertValueToUriLiteral(object value, bool escapeDataString);
        FunctionFormat FunctionFormat { get; }
    }
}