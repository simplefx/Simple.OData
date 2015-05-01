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

        protected override void FormatExpandSelectOrderby(IList<string> commandClauses, EntityCollection resultCollection, FluentCommand command)
        {
            FormatClause(commandClauses, resultCollection, command.Details.ExpandAssociations, ODataLiteral.Expand, FormatExpandItem);
            FormatClause(commandClauses, resultCollection, command.Details.SelectColumns, ODataLiteral.Select, FormatSelectItem);
            FormatClause(commandClauses, resultCollection, command.Details.OrderbyColumns, ODataLiteral.OrderBy, FormatOrderByItem);
        }

        protected override void FormatInlineCount(IList<string> commandClauses)
        {
            commandClauses.Add(string.Format("{0}={1}", ODataLiteral.InlineCount, ODataLiteral.AllPages));
        }
    }
}