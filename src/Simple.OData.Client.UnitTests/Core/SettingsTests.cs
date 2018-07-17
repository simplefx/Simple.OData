using System;
using System.Linq.Expressions;
using System.Net.Http;
using Xunit;

namespace Simple.OData.Client.Tests.Core
{
    public class SettingsTests
    {
        [Fact]
        public void DefaultCtorAndBaseUri()
        {
            var settings = new ODataClientSettings {BaseUri = new Uri("http://localhost")};
            Assert.Equal("http://localhost/", settings.BaseUri.AbsoluteUri);
        }

        [Fact]
        public void CtorWithBaseUri()
        {
            var settings = new ODataClientSettings(new Uri("http://localhost"));
            Assert.Equal("http://localhost/", settings.BaseUri.AbsoluteUri);
        }

        [Fact]
        public void CtorWithHttpClient()
        {
            var settings = new ODataClientSettings(new HttpClient());
            Assert.Null(settings.BaseUri);
        }

        [Fact]
        public void CtorWithHttpClientNoBaseAddressAndBaseUri()
        {
            var settings = new ODataClientSettings(new HttpClient()) {BaseUri = new Uri("http://localhost")};
            Assert.Equal("http://localhost/", settings.BaseUri.AbsoluteUri);
        }

        [Fact]
        public void CtorWithHttpClientNoBaseAddressAndRelativeUri()
        {
            Assert.Throws<ArgumentException>(() => new ODataClientSettings(new HttpClient(), new Uri("api", UriKind.Relative)));
        }

        [Fact]
        public void CtorWithHttpClientAndBaseAddres()
        {
            var settings = new ODataClientSettings(new HttpClient {BaseAddress = new Uri("http://localhost")});
            Assert.Equal("http://localhost/", settings.BaseUri.AbsoluteUri);
        }

        [Fact]
        public void CtorWithHttpClientAndRelativeBaseAddres()
        {
            Assert.Throws<ArgumentException>(() => new ODataClientSettings(new HttpClient {BaseAddress = new Uri("abc", UriKind.Relative)}));
        }

        [Fact]
        public void CtorWithHttpClientAndBaseAddresAndRelativeUrl()
        {
            var settings = new ODataClientSettings(new HttpClient {BaseAddress = new Uri("http://localhost")}, new Uri("api", UriKind.Relative));
            Assert.Equal("http://localhost/api", settings.BaseUri.AbsoluteUri);
        }

        [Fact]
        public void CtorWithHttpClientAndBaseAddresAndAbsoluteUrl()
        {
            Assert.Throws<ArgumentException>(() => new ODataClientSettings(new HttpClient {BaseAddress = new Uri("http://localhost")}, new Uri("http://localhost/api")));
        }
    }
}
