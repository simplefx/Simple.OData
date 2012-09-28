using System;
using System.Runtime.Serialization;

namespace Simple.OData.Client
{
#if !NETFX_CORE
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

#if !NETFX_CORE
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
            get { return Data.Contains("ObjectName") ? Data["ObjectName"].ToString() : null; }
            private set { Data["ObjectName"] = value; }
        }
    }
}
