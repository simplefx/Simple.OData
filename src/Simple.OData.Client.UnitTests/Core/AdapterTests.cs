using System;
using Xunit;

namespace Simple.OData.Client.Tests.Core
{
    public class AdapterTests : TestBase
    {
        class CustomAdapter : V4.Adapter.ODataAdapter
        {
            public CustomAdapter(ISession session, IODataModelAdapter modelAdapter) : base(session, modelAdapter)
            {
            }
        }
        class CustomAdapterFactory : AdapterFactory
        {
            public override Func<ISession, IODataAdapter> CreateAdapterLoader(string metadataString, ITypeCache typeCache)
            {
                var modelAdapter = CreateModelAdapter(metadataString, typeCache);
                return session => new CustomAdapter(session, modelAdapter);
            }
        }
        [Fact]
        public void UseCustomAdapter()
        {
            var settings = CreateDefaultSettings();
            settings.AdapterFactory = new CustomAdapterFactory();
            var client = new ODataClient(settings);
            Assert.IsType<CustomAdapter>(client.Session.Adapter);
        }
    }
}
