using System;
using System.Net;
using System.Runtime.Serialization;
using Simple.NExtLib.Xml;

namespace Simple.OData.Client
{
#if !NETFX_CORE
    [Serializable]
#endif
    public class WebRequestException : Exception
    {
        private readonly string _code;

        public static WebRequestException CreateFromWebException(WebException ex)
        {
            var xml = GetResponseBodyXml(ex.Response);
            if (xml == null) return new WebRequestException(ex);
            return new WebRequestException(xml["message"].Value, xml["code"].Value, ex);
        }

        public WebRequestException(string message)
            : base(message)
        {
        }

        public WebRequestException(string message, string code)
            : base(message)
        {
            _code = code;
        }

        public WebRequestException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public WebRequestException(string message, string code, Exception inner)
            : base(message, inner)
        {
            _code = code;
        }

        public WebRequestException(WebException inner)
            : base("Unexpected WebException encountered", inner)
        {

        }

#if !NETFX_CORE
        protected WebRequestException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
#endif

        public string Code
        {
            get { return _code; }
        }

        private static XmlElementAsDictionary GetResponseBodyXml(WebResponse response)
        {
            if (response == null) return null;

            var stream = response.GetResponseStream();
            if (stream == null || !stream.CanRead) return null;

            return XmlElementAsDictionary.Parse(stream);
        }
    }
}
