using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Simple.OData.Client
{
    public class Function
    {
        private readonly string _actualName;
        private readonly string _httpMethod;
        private readonly string _tableName;
        private readonly string _returnType;
        private Collection<string> _parameters;

        public Function(string name, string httpMethod, string tableName, string returnType, IEnumerable<string> parameters)
        {
            _actualName = name;
            _httpMethod = httpMethod;
            _tableName = tableName;
            _returnType = returnType;
            _parameters = new Collection<string>(parameters.ToList());
        }

        public override string ToString()
        {
            return _actualName;
        }

        internal string HomogenizedName
        {
            get { return _actualName.Homogenize(); }
        }

        public string ActualName
        {
            get { return _actualName; }
        }

        public string HomogenizedTableName
        {
            get { return TableName.Homogenize(); }
        }

        public string TableName
        {
            get { return _tableName; }
        }

        public string ReturnType
        {
            get { return _returnType; }
        }

        public string HttpMethod
        {
            get { return _httpMethod; }
        }

        public Collection<string> Parameters
        {
            get { return _parameters; }
        }
    }
}
