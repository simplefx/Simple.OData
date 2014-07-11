using System;
using System.Net;
using System.Runtime.Serialization;

namespace Simple.OData.Client
{
#if NET40
    [Serializable]
#endif
    /// <summary>
    /// The exception that is thrown when the service failed to process the Web request
    /// </summary>
    public class WebRequestException : Exception
    {
        private readonly HttpStatusCode _code;
        private readonly string _response;

        /// <summary>
        /// Creates from the instance of WebException.
        /// </summary>
        /// <param name="ex">The instance of <see cref="WebException"/>.</param>
        /// <returns>The instance of <see cref="WebRequestException"/>.</returns>
        public static WebRequestException CreateFromWebException(WebException ex)
        {
            var response = ex.Response as HttpWebResponse;
            return response == null ?
                new WebRequestException(ex) :
                new WebRequestException(ex.Message, response, ex);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebRequestException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public WebRequestException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebRequestException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="code">The <see cref="HttpStatusCode"/>.</param>
        public WebRequestException(string message, HttpStatusCode code)
            : base(message)
        {
            _code = code;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebRequestException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The inner exception.</param>
        public WebRequestException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebRequestException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="response">The <see cref="HttpWebResponse"/>.</param>
        /// <param name="inner">The inner exception.</param>
        public WebRequestException(string message, HttpWebResponse response, Exception inner)
            : base(message, inner)
        {
            _code = response.StatusCode;
            _response = Utils.StreamToString(response.GetResponseStream());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebRequestException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="code">The <see cref="HttpStatusCode"/>.</param>
        /// <param name="inner">The inner exception.</param>
        public WebRequestException(string message, HttpStatusCode code, Exception inner)
            : base(message, inner)
        {
            _code = code;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebRequestException"/> class.
        /// </summary>
        /// <param name="inner">The inner exception.</param>
        public WebRequestException(WebException inner)
            : base("Unexpected WebException encountered", inner)
        {
        }

#if NET40
        protected WebRequestException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
#endif

        /// <summary>
        /// Gets the <see cref="HttpStatusCode"/>.
        /// </summary>
        /// <value>
        /// The <see cref="HttpStatusCode"/>.
        /// </value>
        public HttpStatusCode Code
        {
            get { return _code; }
        }

        /// <summary>
        /// Gets the HTTP response text.
        /// </summary>
        /// <value>
        /// The response text.
        /// </value>
        public string Response
        {
            get { return _response; }
        }
    }
}
