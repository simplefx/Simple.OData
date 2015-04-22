using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;

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

        public override void FormatCommandClauses(
            IList<string> commandClauses,
            EntityCollection entityCollection,
            IList<KeyValuePair<string, ODataExpandOptions>> expandAssociations,
            IList<string> selectColumns,
            IList<KeyValuePair<string, bool>> orderbyColumns,
            bool includeCount)
        {
            if (expandAssociations.Any())
            {
                commandClauses.Add(string.Format("{0}={1}", ODataLiteral.Expand,
                    string.Join(",", expandAssociations.Select(x =>
                        FormatExpansionSegment(x.Key, entityCollection,
                        x.Value,
                        SelectExpansionSegmentColumns(selectColumns, x.Key),
                        SelectExpansionSegmentColumns(orderbyColumns, x.Key))))));
            }

            selectColumns = SelectExpansionSegmentColumns(selectColumns, null);
            FormatClause(commandClauses, entityCollection, selectColumns, ODataLiteral.Select, FormatSelectItem);

            orderbyColumns = SelectExpansionSegmentColumns(orderbyColumns, null);
            FormatClause(commandClauses, entityCollection, orderbyColumns, ODataLiteral.OrderBy, FormatOrderByItem);

            if (includeCount)
            {
                commandClauses.Add(string.Format("{0}={1}", ODataLiteral.Count, ODataLiteral.True));
            }
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
                    SelectExpansionSegmentColumns(selectColumns, associationName),
                    SelectExpansionSegmentColumns(orderbyColumns, associationName))));
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