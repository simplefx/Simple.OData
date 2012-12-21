using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Simple.Data.OData
{
    using System.Security;

    [Serializable]
    public class ODataAdapterException : AdapterException
    {
        public ODataAdapterException()
            : base(typeof(ODataTableAdapter))
        {
        }

        public ODataAdapterException(string message, IDbCommand command)
            : base(message, typeof(ODataTableAdapter))
        {
            CommandText = command.CommandText;
            Parameters = command.Parameters.Cast<IDbDataParameter>()
                .ToDictionary(p => p.ParameterName, p => p.Value);
        }

        public ODataAdapterException(string commandText, IEnumerable<KeyValuePair<string, object>> parameters)
            : base(typeof(ODataTableAdapter))
        {
            CommandText = commandText;
            Parameters = parameters.ToDictionary();
        }


        public ODataAdapterException(string message)
            : base(message, typeof(ODataTableAdapter))
        {
        }

        public ODataAdapterException(string message, string commandText, IEnumerable<KeyValuePair<string, object>> parameters)
            : base(message, typeof(ODataTableAdapter))
        {
            CommandText = commandText;
            Parameters = parameters.ToDictionary();
        }

        public ODataAdapterException(string message, Exception inner)
            : base(message, inner, typeof(ODataTableAdapter))
        {
        }

        protected ODataAdapterException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public IDictionary<string, object> Parameters
        {
            get { return Data.Contains("Parameters") ? ((KeyValuePair<string, object>[])Data["Parameters"]).ToDictionary() : null; }
            private set { Data["Parameters"] = value.ToArray(); }
        }

        public string CommandText
        {
            get { return Data.Contains("CommandText") ? Data["CommandText"].ToString() : null; }
            private set { Data["CommandText"] = value; }
        }
    }
}
