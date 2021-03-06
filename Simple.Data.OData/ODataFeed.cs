﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using Simple.OData.Client;

namespace Simple.Data.OData
{
    public class ODataFeed
    {
        private readonly ODataClientSettings _clientSettings = new ODataClientSettings();

        internal ODataClientSettings ClientSettings
        {
            get { return _clientSettings; }
        }

        public string Url
        {
            get { return _clientSettings.UrlBase; }
            set { _clientSettings.UrlBase = value; }
        }

        public ICredentials Credentials
        {
            get { return _clientSettings.Credentials; }
            set { _clientSettings.Credentials = value; }
        }

        public bool IncludeResourceTypeInEntryProperties
        {
            get { return _clientSettings.IncludeResourceTypeInEntryProperties; }
            set { _clientSettings.IncludeResourceTypeInEntryProperties = value; }
        }

        public bool IgnoreResourceNotFoundException
        {
            get { return _clientSettings.IgnoreResourceNotFoundException; }
            set { _clientSettings.IgnoreResourceNotFoundException = value; }
        }

        public Action<HttpRequestMessage> BeforeRequest
        {
            get { return _clientSettings.BeforeRequest; }
            set { _clientSettings.BeforeRequest = value; }
        }

        public Action<HttpResponseMessage> AfterResponse
        {
            get { return _clientSettings.AfterResponse; }
            set { _clientSettings.AfterResponse = value; }
        }

        public const string ResourceTypeLiteral = "__resourcetype";

        public ODataFeed()
        {
        }

        public ODataFeed(string url, ICredentials credentials = null)
        {
            this.Url = url;
            this.Credentials = credentials;
        }

        internal ODataFeed(dynamic expando)
        {
            this.Url = expando.Url;
            this.Credentials = expando.Credentials;
            this.IncludeResourceTypeInEntryProperties = expando.IncludeResourceTypeInEntryProperties;
            this.IgnoreResourceNotFoundException = expando.IgnoreResourceNotFoundException;
            this.BeforeRequest = expando.BeforeRequest;
            this.AfterResponse = expando.AfterResponse;
        }
    }
}