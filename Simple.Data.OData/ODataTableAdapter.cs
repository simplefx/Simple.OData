using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Simple.OData.Client;

namespace Simple.Data.OData
{
    [Export("OData", typeof(Adapter))]
    public partial class ODataTableAdapter : Adapter
    {
        private string _urlBase;
        private ISchema _schema;

        internal string UrlBase
        {
            get { return _urlBase; }
        }

        internal ISchema GetSchema()
        {
            return _schema ?? (_schema = ODataClient.GetSchema(_urlBase));
        }

        protected override void OnSetup()
        {
            base.OnSetup();

            _urlBase = Settings.Url;
            _schema = ODataClient.GetSchema(_urlBase);
        }

        public override IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            return FindByExpression(tableName, criteria);
        }

        public override IDictionary<string, object> GetKey(string tableName, IDictionary<string, object> record)
        {
            return GetTable(tableName).GetKey(tableName, record);
        }

        public override IList<string> GetKeyNames(string tableName)
        {
            return GetTable(tableName).GetKeyNames();
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
            CheckInsertablePropertiesAreAvailable(tableName, data);
            return GetODataClient().InsertEntry(tableName, data, resultRequired);
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
            var cmd = new CommandBuilder().BuildCommand(tableName, criteria);
            return GetODataClientCommand(cmd).FindEntries();
        }

        private IEnumerable<IDictionary<string, object>> FindByQuery(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            var cmd = new CommandBuilder().BuildCommand(query);
            unhandledClauses = cmd.UnprocessedClauses;
            var clientCommand = GetODataClientCommand(cmd);

            IEnumerable<IDictionary<string, object>> results;
            if (cmd.SetTotalCount == null)
            {
                results = clientCommand.FindEntries(cmd.IsScalarResult);
            }
            else
            {
                int totalCount;
                results = clientCommand.FindEntries(out totalCount);
                cmd.SetTotalCount(totalCount);
            }
            return results;
        }

        private IDictionary<string, object> FindByKey(string tableName, object[] keyValues)
        {
            var cmd = new CommandBuilder().BuildCommand(tableName, keyValues);
            return GetODataClientCommand(cmd).FindEntries().SingleOrDefault();
        }

        private int UpdateByExpression(string tableName, IDictionary<string, object> data, SimpleExpression criteria, IAdapterTransaction transaction)
        {
            var cmd = new CommandBuilder().BuildCommand(tableName, criteria);
            var clientCommand = GetODataClientCommand(cmd);
            return GetODataClient(transaction).UpdateEntries(tableName, clientCommand.CommandText, data);
        }

        private int DeleteByExpression(string tableName, SimpleExpression criteria, IAdapterTransaction transaction)
        {
            var cmd = new CommandBuilder().BuildCommand(tableName, criteria);
            var clientCommand = GetODataClientCommand(cmd);
            return GetODataClient(transaction).DeleteEntries(tableName, clientCommand.CommandText);
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
                client = new ODataClient(_urlBase);
            }

            var adapterPluralizer = Database.GetPluralizer();
            if (adapterPluralizer != null)
            {
                var clientPluralizer = new Pluralizer(adapterPluralizer.IsPlural, adapterPluralizer.IsSingular,
                                                      adapterPluralizer.Pluralize, adapterPluralizer.Singularize);
                ODataClient.SetPluralizer(clientPluralizer);
            }

            return client;
        }

        private IClientWithCommand GetODataClientCommand(QueryCommand cmd)
        {
            var linkNames = cmd.TablePath.Split('.');
            var client = GetODataClient();
            var clientCommand = client.From(linkNames.First());

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

        private Table GetTable(string tableName)
        {
            return new Table(tableName, GetSchema());
        }

        private void CheckInsertablePropertiesAreAvailable(string tableName, IEnumerable<KeyValuePair<string, object>> data)
        {
            var table = GetSchema().FindTable(tableName);
            data = data.Where(kvp => table.HasColumn(kvp.Key));

            if (!data.Any())
            {
                throw new SimpleDataException("No properties were found which could be mapped to the database.");
            }
        }
    }
}
