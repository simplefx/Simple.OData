using System;
using System.Collections.Generic;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query;

namespace Simple.OData.Client.V3.Adapter
{
    public class CommandFormatter : CommandFormatterBase
    {
        public CommandFormatter(ISession session)
            : base(session)
        {            
        }

        public override FunctionFormat FunctionFormat
        {
            get { return FunctionFormat.Query; }
        }

        public override void FormatCommandClauses(
            IList<string> commandClauses,
            EntityCollection entityCollection,
            IList<KeyValuePair<string, ODataExpandOptions>> expandAssociations,
            IList<string> selectColumns,
            IList<KeyValuePair<string, bool>> orderbyColumns,
            bool includeCount)
        {
            FormatClause(commandClauses, entityCollection, expandAssociations, ODataLiteral.Expand, FormatExpandItem);
            FormatClause(commandClauses, entityCollection, selectColumns, ODataLiteral.Select, FormatSelectItem);
            FormatClause(commandClauses, entityCollection, orderbyColumns, ODataLiteral.OrderBy, FormatOrderByItem);

            if (includeCount)
            {
                commandClauses.Add(string.Format("{0}={1}", ODataLiteral.InlineCount, ODataLiteral.AllPages));
            }
        }
    }
}