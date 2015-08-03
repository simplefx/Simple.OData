using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client.V4.Adapter
{
    public class CommandFormatter : CommandFormatterBase
    {
        public CommandFormatter(ISession session)
            : base(session)
        {
        }

        public override FunctionFormat FunctionFormat
        {
            get { return FunctionFormat.Key; }
        }

        public override string ConvertValueToUriLiteral(object value, bool escapeDataString)
        {
            if (value != null && value.GetType().IsEnumType())
                value = new ODataEnumValue(value.ToString(), _session.Metadata.GetQualifiedTypeName(value.GetType().Name));

            var odataVersion = (ODataVersion)Enum.Parse(typeof(ODataVersion), _session.Adapter.GetODataVersionString(), false);

            Func<object, string> convertValue = x => ODataUriUtils.ConvertToUriLiteral(x, odataVersion, (_session.Adapter as ODataAdapter).Model);

            return value is ODataExpression
                ? (value as ODataExpression).AsString(_session)
                : escapeDataString
                ? Uri.EscapeDataString(convertValue(value))
                : convertValue(value);
        }

        protected override void FormatExpandSelectOrderby(IList<string> commandClauses, EntityCollection resultCollection, FluentCommand command)
        {
            if (command.Details.ExpandAssociations.Any())
            {
                commandClauses.Add(string.Format("{0}={1}", ODataLiteral.Expand,
                    string.Join(",", command.Details.ExpandAssociations.Select(x =>
                        FormatExpansionSegment(x.Key, resultCollection,
                        x.Value,
                        SelectExpansionSegmentColumns(command.Details.SelectColumns, x.Key),
                        SelectExpansionSegmentColumns(command.Details.OrderbyColumns, x.Key))))));
            }

            FormatClause(commandClauses, resultCollection, 
                SelectExpansionSegmentColumns(command.Details.SelectColumns, null), 
                ODataLiteral.Select, FormatSelectItem);

            FormatClause(commandClauses, resultCollection, 
                SelectExpansionSegmentColumns(command.Details.OrderbyColumns, null), 
                ODataLiteral.OrderBy, FormatOrderByItem);
        }

        protected override void FormatInlineCount(IList<string> commandClauses)
        {
            commandClauses.Add(string.Format("{0}={1}", ODataLiteral.Count, ODataLiteral.True));
        }

        private string FormatExpansionSegment(string path, EntityCollection entityCollection,
            ODataExpandOptions expandOptions, IList<string> selectColumns, IList<KeyValuePair<string, bool>> orderbyColumns)
        {
            var items = path.Split('/');
            var associationName = _session.Metadata.GetNavigationPropertyExactName(entityCollection.Name, items.First());

            var clauses = new List<string>();
            var text = associationName;
            if (expandOptions.ExpandMode == ODataExpandMode.ByReference)
                text += "/" + ODataLiteral.Ref;

            if (items.Count() > 1)
            {
                path = path.Substring(items.First().Length + 1);
                entityCollection = _session.Metadata.GetEntityCollection(
                    _session.Metadata.GetNavigationPropertyPartnerName(entityCollection.Name, associationName));

                clauses.Add(string.Format("{0}={1}", ODataLiteral.Expand, 
                    FormatExpansionSegment(path, entityCollection, expandOptions,
                        SelectExpansionSegmentColumns(selectColumns, path),
                        SelectExpansionSegmentColumns(orderbyColumns, path))));
            }

            if (expandOptions.Levels > 1)
            {
                clauses.Add(string.Format("{0}={1}", ODataLiteral.Levels, expandOptions.Levels));
            }
            else if (expandOptions.Levels == 0)
            {
                clauses.Add(string.Format("{0}={1}", ODataLiteral.Levels, ODataLiteral.Max));
            }

            if (selectColumns.Any())
            {
                var columns = string.Join(",", SelectExpansionSegmentColumns(selectColumns, null));
                if (!string.IsNullOrEmpty(columns))
                    clauses.Add(string.Format("{0}={1}", ODataLiteral.Select, columns));
            }

            if (orderbyColumns.Any())
            {
                var columns = string.Join(",", SelectExpansionSegmentColumns(orderbyColumns, null)
                    .Select(x => x.Key + (x.Value ? " desc" : string.Empty)).ToList());
                if (!string.IsNullOrEmpty(columns))
                    clauses.Add(string.Format("{0}={1}", ODataLiteral.OrderBy, columns));
            }

            if (clauses.Any())
                text += string.Format("({0})", string.Join(";", clauses));

            return text;
        }

        private IList<string> SelectExpansionSegmentColumns(
            IList<string> columns, string path)
        {
            if (string.IsNullOrEmpty(path))
                return columns.Where(x => !x.Contains("/")).ToList();
            else
                return columns.Where(x => x.Contains("/") && x.Split('/').First() == path.Split('/').First())
                    .Select(x => string.Join("/", x.Split('/').Skip(1))).ToList();
        }

        private IList<KeyValuePair<string, bool>> SelectExpansionSegmentColumns(
            IList<KeyValuePair<string, bool>> columns, string path)
        {
            if (string.IsNullOrEmpty(path))
                return columns.Where(x => !x.Key.Contains("/")).ToList();
            else
                return columns.Where(x => x.Key.Contains("/") && x.Key.Split('/').First() == path.Split('/').First())
                    .Select(x => new KeyValuePair<string, bool>(
                        string.Join("/", x.Key.Split('/').Skip(1)), x.Value)).ToList();
        }
    }
}