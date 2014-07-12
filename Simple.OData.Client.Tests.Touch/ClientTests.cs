using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;

namespace Simple.OData.Client.Tests
{
	[TestFixture]
    public class ClientTests
    {
        [Test]
		public void CheckODataOrgNorthwindSchema()
        {
			AsyncContext.Run (async () => 
				{
					var client = new ODataClient ("http://services.odata.org/V2/Northwind/Northwind.svc/");

					var schema = await client.GetSchemaAsync ();
					var table = schema.FindTable ("Product");
					Assert.AreEqual ("ProductID", table.PrimaryKey [0]);

					var association = table.FindAssociation ("Category");
					Assert.AreEqual ("Categories", association.ReferenceTableName);
					Assert.AreEqual ("0..1", association.Multiplicity);

					table = schema.FindTable ("Employees");
					association = table.FindAssociation ("Employee1");
					Assert.AreEqual ("Employees", association.ReferenceTableName);
					Assert.AreEqual ("0..1", association.Multiplicity);

					Assert.AreEqual (26, schema.EntityTypes.Count ());
					Assert.AreEqual (0, schema.ComplexTypes.Count ());
				});
		}

		[Test]
		public void CheckODataOrgODataSchema()
        {
			AsyncContext.Run (async () => 
				{
					var client = new ODataClient("http://services.odata.org/V3/OData/OData.svc/");

					var schema = await client.GetSchemaAsync();
					var table = schema.FindTable("Product");
					Assert.AreEqual("ID", table.PrimaryKey[0]);

					var association = table.FindAssociation("Category");
					Assert.AreEqual("Categories", association.ReferenceTableName);
					Assert.AreEqual("*", association.Multiplicity);

					var function = schema.FindFunction("GetProductsByRating");
					Assert.AreEqual("GET", function.HttpMethod);
					Assert.AreEqual("rating", function.Parameters[0]);

					Assert.AreEqual(10, schema.EntityTypes.Count());
					Assert.AreEqual(1, schema.ComplexTypes.Count());
					Assert.AreEqual(5, schema.ComplexTypes.First().Properties.Count());
				});
        }

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
    }
}
