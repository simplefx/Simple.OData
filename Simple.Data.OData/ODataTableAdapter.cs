﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Simple.OData.Client;

namespace Simple.Data.OData
{
    [Export("OData", typeof(Adapter))]
    public partial class ODataTableAdapter : Adapter
    {
        private ODataClientSettings _clientSettings;
        private ISchema _schema;

        internal ODataClientSettings ClientSettings
        {
            get { return _clientSettings; }
        }

        internal ISchema GetSchema()
        {
            return _schema ?? (_schema = Utils.ExecuteAndUnwrap(() => 
                ODataClient.GetSchemaAsync(_clientSettings.UrlBase, _clientSettings.Credentials)));
        }

        protected override void OnSetup()
        {
            base.OnSetup();

            var odataFeed = new ODataFeed(this.Settings);
            _clientSettings = odataFeed.ClientSettings;
            _schema = Utils.ExecuteAndUnwrap(() => 
                ODataClient.GetSchemaAsync(_clientSettings.UrlBase, _clientSettings.Credentials));
        }

        public override IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            return FindByExpression(tableName, criteria);
        }

        public override IDictionary<string, object> GetKey(string tableName, IDictionary<string, object> record)
        {
            return FindTable(tableName).GetKey(tableName, record);
        }

        public override IList<string> GetKeyNames(string tableName)
        {
            return FindTable(tableName).GetKeyNames();
        }

        public override IDictionary<string, object> Get(string tableName, params object[] keyValues)
        {
            return FindByKey(tableName, keyValues);
        }

        public override IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            return FindByQuery(query, out unhandledClauses);
        }

        public override IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data, bool resultRequired)
        {
            return InsertOne(tableName, data, resultRequired, null);
        }

        public override int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria)
        {
            return UpdateByExpression(tableName, data, criteria, null);
        }

        public override int Delete(string tableName, SimpleExpression criteria)
        {
            return DeleteByExpression(tableName, criteria, null);
        }

        public override bool IsExpressionFunction(string functionName, params object[] args)
        {
            return false;
        }

        private IEnumerable<IDictionary<string, object>> FindByExpression(string tableName, SimpleExpression criteria)
        {
            var baseTable = GetBaseTable(tableName);
            var concreteTableName = baseTable == null ? tableName : baseTable.ActualName;

            var cmd = GetCommandBuilder().BuildCommand(concreteTableName, criteria);
            return Utils.ExecuteAndUnwrap(() => 
                GetODataClientCommand(cmd).FindEntriesAsync());
        }

        private IEnumerable<IDictionary<string, object>> FindByQuery(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            var cmd = GetCommandBuilder().BuildCommand(query);
            unhandledClauses = cmd.UnprocessedClauses;
            var clientCommand = GetODataClientCommand(cmd);

            IEnumerable<IDictionary<string, object>> results;
            if (cmd.SetTotalCount == null)
            {
                results = Utils.ExecuteAndUnwrap(() => 
                    clientCommand.FindEntriesAsync(cmd.IsScalarResult));
            }
            else
            {
                var resultsWithCount = Utils.ExecuteAndUnwrap(clientCommand.FindEntriesWithCountAsync);
                results = resultsWithCount.Item1;
                cmd.SetTotalCount(resultsWithCount.Item2);
            }
            return results;
        }

        private IDictionary<string, object> FindByKey(string tableName, object[] keyValues)
        {
            var baseTable = GetBaseTable(tableName);
            var concreteTableName = baseTable == null ? tableName : baseTable.ActualName;

            var cmd = GetCommandBuilder().BuildCommand(concreteTableName, keyValues);
            return Utils.ExecuteAndUnwrap(() => 
                GetODataClientCommand(cmd).FindEntriesAsync()).SingleOrDefault();
        }

        private IDictionary<string, object> InsertOne(string tableName, 
            IDictionary<string, object> data, bool resultRequired, IAdapterTransaction transaction)
        {
            tableName = EvaluateConcreteTableName(tableName, data, null);
            CheckInsertablePropertiesAreAvailable(tableName, data);
            var tablePath = GetTablePath(tableName);

            var client = GetODataClient(transaction);

            return Utils.ExecuteAndUnwrap(() => 
                client.InsertEntryAsync(tablePath, data, resultRequired));
        }

        private int UpdateByExpression(string tableName, 
            IDictionary<string, object> data, SimpleExpression criteria, IAdapterTransaction transaction)
        {
            tableName = EvaluateConcreteTableName(tableName, data, criteria);
            var tablePath = GetTablePath(tableName);
            var concreteTableName = tablePath.Split('/').Last();

            var cmd = GetCommandBuilder().BuildCommand(concreteTableName, criteria);
            var clientCommand = GetODataClientCommand(cmd);
            var client = GetODataClient(transaction);

            int result;
            if (clientCommand.FilterIsKey)
            {
                Utils.ExecuteAndUnwrap(() => 
                    client.UpdateEntryAsync(tablePath, clientCommand.FilterAsKey, data));
                result = 1;
            }
            else
            {
                result = Utils.ExecuteAndUnwrap(() => 
                    client.UpdateEntriesAsync(tablePath, Utils.ExecuteAndUnwrap(clientCommand.GetCommandTextAsync), data)).Count();
            }
            return result;
        }

        private int DeleteByExpression(string tableName, 
            SimpleExpression criteria, IAdapterTransaction transaction)
        {
            tableName = EvaluateConcreteTableName(tableName, null, criteria);
            var tablePath = GetTablePath(tableName);
            var concreteTableName = tablePath.Split('/').Last();

            var cmd = GetCommandBuilder().BuildCommand(concreteTableName, criteria);
            var clientCommand = GetODataClientCommand(cmd);
            var client = GetODataClient(transaction);
                
            int result;
            if (clientCommand.FilterIsKey)
            {
                Utils.ExecuteAndUnwrap(() => 
                    client.DeleteEntryAsync(tablePath, clientCommand.FilterAsKey));
                result = 1;
            }
            else
            {
                result = Utils.ExecuteAndUnwrap(() => 
                    client.DeleteEntriesAsync(tablePath, Utils.ExecuteAndUnwrap(clientCommand.GetCommandTextAsync)));
            }
            return result;
        }

        private ODataClient GetODataClient(IAdapterTransaction transaction = null)
        {
            ODataClient client;
            var adapterTransaction = transaction as ODataAdapterTransaction;
            if (adapterTransaction != null)
            {
                client = new ODataClient(adapterTransaction.Batch);
            }
            else
            {
                client = new ODataClient(_clientSettings);
            }

            var adapterPluralizer = Database.GetPluralizer();
            if (adapterPluralizer != null)
            {
                var clientPluralizer = new Pluralizer(adapterPluralizer.Pluralize, adapterPluralizer.Singularize);
                ODataClient.SetPluralizer(clientPluralizer);
            }

            return client;
        }

        private IFluentClient<IDictionary<string, object>> GetODataClientCommand(QueryCommand cmd)
        {
            var linkNames = cmd.TablePath.Split('.');
            var client = GetODataClient();

            var tableName = linkNames.First();
            var baseTable = GetBaseTable(tableName);
            var derivedTable = GetAsDerivedTable(tableName);
            var clientCommand = derivedTable != null
                ? client.For(baseTable.ActualName).As(derivedTable.ActualName)
                : client.For(tableName);

            if (cmd.NamedKeyValues != null && cmd.NamedKeyValues.Count > 0)
                clientCommand = clientCommand.Key(cmd.NamedKeyValues);
            else if (cmd.KeyValues != null && cmd.KeyValues.Count > 0)
                clientCommand = clientCommand.Key(cmd.KeyValues);

            if (!ReferenceEquals(cmd.FilterExpression, null))
                clientCommand = clientCommand.Filter(cmd.FilterExpression);

            if (cmd.Expand.Count > 0)
                clientCommand = clientCommand.Expand(cmd.Expand);

            if (cmd.SkipCount.HasValue)
                clientCommand = clientCommand.Skip(cmd.SkipCount.Value);

            if (cmd.TakeCount.HasValue)
                clientCommand = clientCommand.Top(cmd.TakeCount.Value);

            if (cmd.Order.Count > 0)
                clientCommand = clientCommand.OrderBy(
                    cmd.Order.Select(x =>
                        new KeyValuePair<string, bool>(x.Reference.GetAliasOrName(),
                        x.Direction == OrderByDirection.Descending)));

            if (cmd.Columns.Count == 1 && cmd.Columns.First().GetType() == typeof(CountSpecialReference))
            {
                clientCommand = clientCommand.Count();
                cmd.IsScalarResult = true;
            }
            else if (cmd.Columns.Count > 0)
            {
                clientCommand = clientCommand.Select(cmd.Columns.Select(x => x.GetAliasOrName()));
            }

            linkNames.Skip(1).ToList().ForEach(x => clientCommand = clientCommand.NavigateTo(x));

            return clientCommand;
        }

        private void CheckInsertablePropertiesAreAvailable(string tableName, IEnumerable<KeyValuePair<string, object>> data)
        {
            Table table = FindTable(tableName);
            data = data.Where(kvp => table.HasColumn(kvp.Key));
            if (!data.Any())
            {
                throw new SimpleDataException("No properties were found which could be mapped to the database.");
            }
        }

        private CommandBuilder GetCommandBuilder()
        {
            return new CommandBuilder(this.ClientSettings.IncludeResourceTypeInEntryProperties);
        }

        private Table FindTable(string tableName)
        {
            Table table = null;
            if (!GetSchema().HasTable(tableName))
                table = GetAsDerivedTable(tableName);
            if (table == null)
                table = GetSchema().FindTable(tableName);
            return table;
        }

        private Table GetBaseTable(string tableName)
        {
            return GetSchema().Tables.SingleOrDefault(x => x.HasDerivedTable(tableName));
        }

        private Table GetAsDerivedTable(string tableName)
        {
            var baseTable = GetBaseTable(tableName);
            return baseTable == null ? null : baseTable.FindDerivedTable(tableName);
        }

        private string GetTablePath(string tableName)
        {
            var baseTable = GetBaseTable(tableName);
            var derivedTable = GetAsDerivedTable(tableName);
            var baseTableName = baseTable == null ? tableName : baseTable.ActualName;
            var derivedTableName = derivedTable == null ? null : derivedTable.ActualName;
            return derivedTableName == null ? baseTableName : string.Join("/", baseTableName, derivedTableName);
        }

        private string EvaluateConcreteTableName(string tableName, IDictionary<string, object> data, SimpleExpression criteria)
        {
            if (!this.ClientSettings.IncludeResourceTypeInEntryProperties)
                return tableName;

            if (data != null && data.ContainsKey(ODataFeed.ResourceTypeLiteral))
            {
                tableName = data[ODataFeed.ResourceTypeLiteral].ToString();
                data.Remove(ODataFeed.ResourceTypeLiteral);
            }
            else if (criteria != null)
            {
                var converter = new ExpressionConverter(true);
                var resourceType = converter.GetReferencedResourceType(criteria);
                if (!string.IsNullOrEmpty(resourceType))
                    tableName = resourceType;
            }
            return tableName;
        }
    }
}
