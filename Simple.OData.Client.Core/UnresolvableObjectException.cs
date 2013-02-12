using System;
using System.Runtime.Serialization;

namespace Simple.OData.Client
{
#if NET40
    [Serializable]
#endif
    public sealed class UnresolvableObjectException : Exception
    {
        public UnresolvableObjectException()
        {
        }

        public UnresolvableObjectException(string objectName)
        {
            ObjectName = objectName;
        }

        public UnresolvableObjectException(string objectName, string message)
            : base(message)
        {
            ObjectName = objectName;
        }

        public UnresolvableObjectException(string objectName, string message, Exception inner)
            : base(message, inner)
        {
            ObjectName = objectName;
        }

#if NET40
        private UnresolvableObjectException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
            ObjectName = info.GetString("ObjectName");
        }
#endif
        public string ObjectName
        {
            get { return Data.Contains("ObjectName") ? (Data["ObjectName"] != null ? Data["ObjectName"].ToString() : "{{null}}") : null; }
            private set { Data["ObjectName"] = value; }
        }
    }
}
