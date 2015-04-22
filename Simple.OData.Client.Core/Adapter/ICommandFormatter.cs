using System.Collections.Generic;

namespace Simple.OData.Client
{
    public interface ICommandFormatter
    {
        string FormatCommand(FluentCommand command);
        void FormatCommandClauses(IList<string> commandClauses, EntityCollection entityCollection,
            IList<KeyValuePair<string, ODataExpandOptions>> expandAssociations, 
            IList<string> selectColumns, 
            IList<KeyValuePair<string, bool>> orderbyColumns, 
            bool includeCount);

        string ConvertKeyValuesToUriLiteral(IDictionary<string, object> key, bool skipKeyNameForSingleValue);
        FunctionFormat FunctionFormat { get; }
    }
}