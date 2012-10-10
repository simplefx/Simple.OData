using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Simple.OData.Client
{
    public class ODataCommand
    {
        private string _tableName;
        private string _filter;
        private int _skipCount = -1;
        private int _topCount = -1;
        private List<string> _expandAssociations = new List<string>();
        private List<string> _selectColumns = new List<string>();
        private List<KeyValuePair<string, bool>> _orderbyColumns = new List<KeyValuePair<string, bool>>();

        public ODataCommand(string tableName)
        {
            _tableName = tableName;
        }

        public ODataCommand Filter(string filter)
        {
            _filter = filter;
            return this;
        }

        public ODataCommand Skip(int count)
        {
            _skipCount = count;
            return this;
        }

        public ODataCommand Top(int count)
        {
            _topCount = count;
            return this;
        }

        public ODataCommand Expand(IEnumerable<string> associations)
        {
            _expandAssociations = associations.ToList();
            return this;
        }

        public ODataCommand Expand(params string[] associations)
        {
            _expandAssociations = associations.ToList();
            return this;
        }

        public ODataCommand Select(IEnumerable<string> columns)
        {
            _selectColumns = columns.ToList();
            return this;
        }

        public ODataCommand Select(params string[] columns)
        {
            _selectColumns = columns.ToList();
            return this;
        }

        public ODataCommand OrderBy(IEnumerable<string> columns, bool descending = false)
        {
            _orderbyColumns.AddRange(columns.Select(x => new KeyValuePair<string, bool>(x, descending)));
            return this;
        }

        public ODataCommand OrderBy(params string[] columns)
        {
            return OrderBy(columns, false);
        }

        public ODataCommand OrderByDescending(IEnumerable<string> columns)
        {
            return OrderBy(columns, true);
        }

        public ODataCommand OrderByDescending(params string[] columns)
        {
            return OrderBy(columns, true);
        }

        public override string ToString()
        {
            return Format();
        }

        private string Format()
        {
            string commandText = _tableName;
            var extraClauses = new List<string>();

            if (!string.IsNullOrEmpty(_filter))
                extraClauses.Add("$filter=" + HttpUtility.UrlEncode(_filter));

            if (_skipCount >= 0)
                extraClauses.Add("$skip=" + _skipCount.ToString());

            if (_topCount >= 0)
                extraClauses.Add("$top=" + _topCount.ToString());

            if (_expandAssociations.Any())
                extraClauses.Add("$expand=" + string.Join(",", _expandAssociations));

            if (_orderbyColumns.Any())
                extraClauses.Add("$orderby=" + string.Join(",", _orderbyColumns.Select(FormatOrderByItem)));

            if (_selectColumns.Any())
                extraClauses.Add("$select=" + string.Join(",", _selectColumns.Select(FormatSelectItem)));

            if (extraClauses.Any())
                commandText += "?" + string.Join("&", extraClauses);

            return commandText;
        }

        private string FormatSelectItem(string item)
        {
            return item;
        }

        private string FormatOrderByItem(KeyValuePair<string,bool> item)
        {
            return item.Key + (item.Value ? " desc" : string.Empty);
        }
    }
}
