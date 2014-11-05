using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;

namespace Simple.OData.Client.Tests
{
	[TestFixture]
    public class ClientTests
    {
		[Test]
		public void AllEntriesFromODataOrg()
        {
			AsyncContext.Run (async () => 
				{
					var client = new ODataClient("http://services.odata.org/V3/OData/OData.svc/");
					var products = await client
						.For("Product")
						.FindEntriesAsync();
					Assert.IsNotNull(products);
					Assert.AreNotEqual(0, products.Count());
				});
        }

		[Test]
		public void DynamicCombinedConditionsFromODataOrg()
        {
			AsyncContext.Run (async () => 
				{
					var client = new ODataClient("http://services.odata.org/V2/OData/OData.svc/");
					var x = ODataDynamic.Expression;
					var product = await client
						.For(x.Product)
						.Filter(x.Name == "Bread" && x.Price < 1000)
						.FindEntryAsync();
					Assert.AreEqual(2.5m, product.Price);
				});
        }

        public class ODataOrgProduct
        {
            public string Name { get; set; }
            public decimal Price { get; set; }
        }

	    [Test]
		public void TypedCombinedConditionsFromODataOrg()
        {
			AsyncContext.Run (async () => 
				{
					var client = new ODataClient("http://services.odata.org/V2/OData/OData.svc/");
					var product = await client
						.For<ODataOrgProduct>("Product")
						.Filter(x => x.Name == "Bread" && x.Price < 1000)
						.FindEntryAsync();
					Assert.AreEqual(2.5m, product.Price);
				});
        }

        public class Product : ODataOrgProduct { }
        public class Products : ODataOrgProduct { }

        [Test]
        public void TypedWithPluralizerFromODataOrg()
        {
            AsyncContext.Run(async () =>
            {
                var client = new ODataClient("http://services.odata.org/V2/OData/OData.svc/");
                var products = await client
                    .For<Product>()
                    .FindEntriesAsync();
                Assert.AreNotEqual(0, products.Count());
            });

            AsyncContext.Run(async () =>
            {
                var client = new ODataClient("http://services.odata.org/V2/OData/OData.svc/");
                var products = await client
                    .For<Products>()
                    .FindEntriesAsync();
                Assert.AreNotEqual(0, products.Count());
            });
        }
    }
}
