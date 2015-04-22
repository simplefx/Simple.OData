using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.OData.Client
{
    public abstract class CommandFormatterBase : ICommandFormatter
    {
        protected readonly ISession _session;

        protected CommandFormatterBase(ISession session)
        {
            _session = session;
        }

        public abstract FunctionFormat FunctionFormat { get; }

        public abstract void FormatCommandClauses(IList<string> commandClauses, EntityCollection entityCollection,
            IList<KeyValuePair<string, ODataExpandOptions>> expandAssociations,
            IList<string> selectColumns,
            IList<KeyValuePair<string, bool>> orderbyColumns, bool includeCount);

        public string FormatCommand(FluentCommand command)
        {
            if (command.HasKey && command.HasFilter)
                throw new InvalidOperationException("OData filter and key may not be combined.");

            if (command.HasFunction && command.HasAction)
                throw new InvalidOperationException("OData function and action may not be combined.");

            var commandText = string.Empty;
            if (!string.IsNullOrEmpty(command.Details.CollectionName))
            {
                commandText += _session.Metadata.GetEntityCollectionExactName(command.Details.CollectionName);
            }
            else if (!string.IsNullOrEmpty(command.Details.LinkName))
            {
                var parent = new FluentCommand(command.Details.Parent).Resolve();
                commandText += string.Format("{0}/{1}",
                    FormatCommand(parent),
                    _session.Metadata.GetNavigationPropertyExactName(parent.EntityCollection.Name, command.Details.LinkName));
            }

            if (command.HasKey)
                commandText += ConvertKeyValuesToUriLiteral(command.KeyValues, true);

            if (!string.IsNullOrEmpty(command.Details.FunctionName) || !string.IsNullOrEmpty(command.Details.ActionName))
            {
                if (!string.IsNullOrEmpty(command.Details.CollectionName) || !string.IsNullOrEmpty(command.Details.LinkName))
                    commandText += "/";
                if (!string.IsNullOrEmpty(command.Details.FunctionName))
                    commandText += _session.Metadata.GetFunctionFullName(command.Details.FunctionName);
                else
                    commandText += _session.Metadata.GetActionFullName(command.Details.ActionName);
            }

            if (!string.IsNullOrEmpty(command.Details.FunctionName) && FunctionFormat == FunctionFormat.Key)
                commandText += ConvertKeyValuesToUriLiteral(command.CommandData, false);

            if (!string.IsNullOrEmpty(command.Details.DerivedCollectionName))
            {
                var entityTypeNamespace = _session.Metadata.GetEntityCollectionTypeNamespace(command.Details.DerivedCollectionName);
                var entityTypeName = _session.Metadata.GetEntityTypeExactName(command.Details.DerivedCollectionName);
                commandText += string.Format("/{0}.{1}", entityTypeNamespace, entityTypeName);
            }

            commandText += FormatClauses(command);

            return commandText;
        }

        public string ConvertKeyValuesToUriLiteral(IDictionary<string, object> key, bool skipKeyNameForSingleValue)
        {
            var formattedKeyValues = key.Count == 1 && skipKeyNameForSingleValue ?
                string.Join(",", key.Select(x => _session.Adapter.ConvertValueToUriLiteral(x.Value))) :
                string.Join(",", key.Select(x => string.Format("{0}={1}", x.Key, _session.Adapter.ConvertValueToUriLiteral(x.Value))));
            return "(" + formattedKeyValues + ")";
        }

        private string FormatClauses(FluentCommand command)
        {
            var text = string.Empty;
            var extraClauses = new List<string>();
            var aggregateClauses = new List<string>();

            if (command.CommandData.Any() && !string.IsNullOrEmpty(command.Details.FunctionName) &&
                FunctionFormat == FunctionFormat.Query)
                extraClauses.Add(string.Join("&", command.CommandData.Select(x => string.Format("{0}={1}",
                    x.Key, _session.Adapter.ConvertValueToUriLiteral(x.Value)))));

            if (command.Details.Filter != null)
                extraClauses.Add(string.Format("{0}={1}", ODataLiteral.Filter, Uri.EscapeDataString(command.Details.Filter)));

            if (command.Details.SkipCount >= 0)
                extraClauses.Add(string.Format("{0}={1}", ODataLiteral.Skip, command.Details.SkipCount));

            if (command.Details.TopCount >= 0)
                extraClauses.Add(string.Format("{0}={1}", ODataLiteral.Top, command.Details.TopCount));

            if (!command.HasAction)
            {
                FormatCommandClauses(
                    extraClauses, command.EntityCollection, command.Details.ExpandAssociations, 
                    command.Details.SelectColumns, command.Details.OrderbyColumns, command.Details.IncludeCount);

                if (command.Details.ComputeCount)
                    aggregateClauses.Add(ODataLiteral.Count);

                if (aggregateClauses.Any())
                    text += "/" + string.Join("/", aggregateClauses);
            }

            if (extraClauses.Any())
                text += "?" + string.Join("&", extraClauses);

            return text;
        }

        protected string FormatExpandItem(KeyValuePair<string, ODataExpandOptions> pathWithOptions, EntityCollection entityCollection)
        {
            return FormatExpandItem(pathWithOptions.Key, entityCollection);
        }

        protected string FormatExpandItem(string path, EntityCollection entityCollection)
        {
            var items = path.Split('/');
            var associationName = _session.Metadata.GetNavigationPropertyExactName(entityCollection.Name, items.First());

            var text = associationName;
            if (items.Count() == 1)
            {
                return text;
            }
            else
            {
                path = path.Substring(items.First().Length + 1);

                entityCollection = _session.Metadata.GetEntityCollection(
                    _session.Metadata.GetNavigationPropertyPartnerName(entityCollection.Name, associationName));

                return string.Format("{0}/{1}", text, FormatExpandItem(path, entityCollection));
            }
        }

        protected string FormatSelectItem(string path, EntityCollection entityCollection)
        {
            var items = path.Split('/');
            if (items.Count() == 1)
            {
                return _session.Metadata.HasStructuralProperty(entityCollection.Name, path)
                    ? _session.Metadata.GetStructuralPropertyExactName(entityCollection.Name, path)
                    : _session.Metadata.HasNavigationProperty(entityCollection.Name, path)
                    ? _session.Metadata.GetNavigationPropertyExactName(entityCollection.Name, path)
                    : path;
            }
            else
            {
                var associationName = _session.Metadata.GetNavigationPropertyExactName(entityCollection.Name, items.First());
                var text = associationName;
                path = path.Substring(items.First().Length + 1);
                entityCollection = _session.Metadata.GetEntityCollection(
                    _session.Metadata.GetNavigationPropertyPartnerName(entityCollection.Name, associationName));
                return string.Format("{0}/{1}", text, FormatSelectItem(path, entityCollection));
            }
        }

        protected string FormatOrderByItem(KeyValuePair<string, bool> pathWithOrder, EntityCollection entityCollection)
        {
            var items = pathWithOrder.Key.Split('/');
            if (items.Count() == 1)
            {
                var clause = _session.Metadata.HasStructuralProperty(entityCollection.Name, pathWithOrder.Key)
                    ? _session.Metadata.GetStructuralPropertyExactName(entityCollection.Name, pathWithOrder.Key)
                    : _session.Metadata.HasNavigationProperty(entityCollection.Name, pathWithOrder.Key)
                    ? _session.Metadata.GetNavigationPropertyExactName(entityCollection.Name, pathWithOrder.Key)
                    : pathWithOrder.Key;
                if (pathWithOrder.Value)
                    clause += " desc";
                return clause;
            }
            else
            {
                var associationName = _session.Metadata.GetNavigationPropertyExactName(entityCollection.Name, items.First());
                var text = associationName;
                var item = pathWithOrder.Key.Substring(items.First().Length + 1);
                entityCollection = _session.Metadata.GetEntityCollection(
                    _session.Metadata.GetNavigationPropertyPartnerName(entityCollection.Name, associationName));
                return string.Format("{0}/{1}", text,
                    FormatOrderByItem(new KeyValuePair<string, bool>(item, pathWithOrder.Value), entityCollection));
            }
        }

        protected void FormatClause<T>(IList<string> commandClauses, EntityCollection entityCollection,
            IList<T> clauses, string clauseLiteral, Func<T, EntityCollection, string> formatItem)
        {
            if (clauses.Any())
            {
                commandClauses.Add(string.Format("{0}={1}", clauseLiteral,
                    string.Join(",", clauses.Select(x => formatItem(x, entityCollection)))));
            }
        }
    }
}