using System.Collections.Generic;

namespace Simple.OData.Client
{
    public interface ICommandFormatter
    {
        string FormatCommand(FluentCommand command);
        string ConvertKeyValuesToUriLiteral(IDictionary<string, object> key, bool skipKeyNameForSingleValue);
        FunctionFormat FunctionFormat { get; }
    }
}