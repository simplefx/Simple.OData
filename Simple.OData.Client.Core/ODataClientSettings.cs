using System;
using System.Net;
using System.Net.Http;

namespace Simple.OData.Client
{
    /// <summary>
    /// OData client configuration settings
    /// </summary>
    public class ODataClientSettings
    {
        /// <summary>
        /// Gets or sets the OData service URL.
        /// </summary>
        /// <value>
        /// The URL address.
        /// </value>
        public string UrlBase { get; set; }

        /// <summary>
        /// Gets or sets the OData client credentials.
        /// </summary>
        /// <value>
        /// The client credentials.
        /// </value>
        public ICredentials Credentials { get; set; }

        /// <summary>
        /// Gets or sets the OData payload format.
        /// </summary>
        /// <value>
        /// The payload format (JSON or Atom).
        /// </value>
        public ODataPayloadFormat PayloadFormat { get; set; }

        /// <summary>
        /// Gets or sets the time period to wait before the request times out.
        /// </summary>
        /// <value>
        /// The timeout.
        /// </value>
        public TimeSpan RequestTimeout { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether entry properties should be extended with the resource type.
        /// </summary>
        /// <value>
        /// <c>true</c> to include resource type in entry properties; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeResourceTypeInEntryProperties { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether resource not found exception (404) should be ignored.
        /// </summary>
        /// <value>
        /// <c>true</c> to ignore resource not found exception; otherwise, <c>false</c>.
        /// </value>
        public bool IgnoreResourceNotFoundException { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether unmapped structural or navigation properties should be ignored or cause <see cref="UnresolvableObjectException"/>.
        /// </summary>
        /// <value>
        /// <c>true</c> to ignore unmapped properties; otherwise, <c>false</c>.
        /// </value>
        public bool IgnoreUnmappedProperties { get; set; }

        /// <summary>
        /// Gets or sets the HttpMessageHandler factory used by HttpClient.
        /// If not set, ODataClient creates an instance of HttpClientHandler.
        /// </summary>
        /// <value>
        /// The action on <see cref="HttpMessageHandler"/>.
        /// </value>
        public Func<HttpMessageHandler> OnCreateMessageHandler { get; set; }

        /// <summary>
        /// Gets or sets the action on HttpClientHandler.
        /// </summary>
        /// <value>
        /// The action on <see cref="HttpClientHandler"/>.
        /// </value>
        public Action<HttpClientHandler> OnApplyClientHandler { get; set; }

        /// <summary>
        /// Gets or sets the action executed before the OData request.
        /// </summary>
        /// <value>
        /// The action on <see cref="HttpRequestMessage"/>.
        /// </value>
        public Action<HttpRequestMessage> BeforeRequest { get; set; }
        
        /// <summary>
        /// Gets or sets the action executed after the OData request.
        /// </summary>
        /// <value>
        /// The action on <see cref="HttpResponseMessage"/>.
        /// </value>
        public Action<HttpResponseMessage> AfterResponse { get; set; }

        /// <summary>
        /// Gets or sets the method that will be executed to write trace messages.
        /// </summary>
        /// <value>
        /// The trace action on <see cref="HttpResponseMessage"/>.
        /// </value>
        public Action<string, object[]> OnTrace { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataClientSettings"/> class.
        /// </summary>
        public ODataClientSettings()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataClientSettings"/> class.
        /// </summary>
        /// <param name="urlBase">The URL address.</param>
        /// <param name="credentials">The client credentials.</param>
        public ODataClientSettings(string urlBase, ICredentials credentials = null)
        {
            this.UrlBase = urlBase;
            this.Credentials = credentials;
        }

        internal ODataClientSettings(ISession session)
        {
            this.UrlBase = session.Settings.UrlBase;
            this.Credentials = session.Settings.Credentials;
            this.PayloadFormat = session.Settings.PayloadFormat;
            this.RequestTimeout = session.Settings.RequestTimeout;
            this.IncludeResourceTypeInEntryProperties = session.Settings.IncludeResourceTypeInEntryProperties;
            this.IgnoreResourceNotFoundException = session.Settings.IgnoreResourceNotFoundException;
            this.IgnoreUnmappedProperties = session.Settings.IgnoreUnmappedProperties;
            this.BeforeRequest = session.Settings.BeforeRequest;
            this.AfterResponse = session.Settings.AfterResponse;
            this.OnCreateMessageHandler = session.Settings.OnCreateMessageHandler;
            this.OnApplyClientHandler = session.Settings.OnApplyClientHandler;
        }
    }
}