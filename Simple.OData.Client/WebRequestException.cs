using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Net;
using Simple.NExtLib.Xml;

namespace Simple.OData.Client
{
    [Serializable]
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

        protected WebRequestException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

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
