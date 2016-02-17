using System;
using System.Collections.Generic;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query;
using Simple.OData.Client.Extensions;

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

        public override string ConvertValueToUriLiteral(object value, bool escapeDataString)
        {
            if (value != null && value.GetType().IsEnumType())
                value = Convert.ToInt32(value);
            if (value is ODataExpression)
                return (value as ODataExpression).AsString(_session);

            var odataVersion = (ODataVersion) Enum.Parse(typeof (ODataVersion), _session.Adapter.GetODataVersionString(), false);
            Func<object, string> convertValue = x => ODataUriUtils.ConvertToUriLiteral(x, odataVersion, (_session.Adapter as ODataAdapter).Model);

            return escapeDataString
                ? Uri.EscapeDataString(convertValue(value))
                : convertValue(value);
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