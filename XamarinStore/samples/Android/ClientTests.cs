using System.Linq;
using NUnit.Framework;

namespace Simple.OData.Client.Tests.Android
{
	[TestFixture]
    public class ClientTests
    {
        [Test]
        public void CheckODataOrgNorthwindSchema()
        {
			var client = new ODataClient("http://services.odata.org/V2/Northwind/Northwind.svc/");

            var table = client.Schema.FindTable("Product");
            Assert.AreEqual("ProductID", table.PrimaryKey[0]);

            var association = table.FindAssociation("Categories");
            Assert.AreEqual("Categories", association.ReferenceTableName);
            Assert.AreEqual("0..1", association.Multiplicity);

            table = client.Schema.FindTable("Employees");
            association = table.FindAssociation("Employees");
            Assert.AreEqual("Employees", association.ReferenceTableName);
            Assert.AreEqual("0..1", association.Multiplicity);

            Assert.AreEqual(26, client.Schema.EntityTypes.Count());
            Assert.AreEqual(0, client.Schema.ComplexTypes.Count());
        }

        [Test]
		public void CheckODataOrgODataSchema()
        {
			var client = new ODataClient("http://services.odata.org/V3/OData/OData.svc/");

            var table = client.Schema.FindTable("Product");
            Assert.AreEqual("ID", table.PrimaryKey[0]);

            var association = table.FindAssociation("Category_Products");
            Assert.AreEqual("Categories", association.ReferenceTableName);
			Assert.AreEqual("*", association.Multiplicity);

            var function = client.Schema.FindFunction("GetProductsByRating");
            Assert.AreEqual(RestVerbs.GET, function.HttpMethod);
            Assert.AreEqual("rating", function.Parameters[0]);

			Assert.AreEqual(10, client.Schema.EntityTypes.Count());
            Assert.AreEqual(1, client.Schema.ComplexTypes.Count());
            Assert.AreEqual(5, client.Schema.ComplexTypes.First().Properties.Count());
        }

        [Test]
        public void AllEntriesFromODataOrg()
        {
            var client = new ODataClient("http://services.odata.org/V3/OData/OData.svc/");
            var products = client
                .For("Product")
                .FindEntries();
            Assert.IsNotNull(products);
            Assert.AreNotEqual(0, products.Count());
        }

        [Test]
        public void DynamicCombinedConditionsFromODataOrg()
        {
			var client = new ODataClient("http://services.odata.org/V2/OData/OData.svc/");
			var x = ODataDynamic.Expression;
			var product = client
				.For(x.Product)
				.Filter(x.Name == "Bread" && x.Price < 1000)
				.FindEntry();
			Assert.AreEqual(2.5m, product.Price);
        }

		public class ODataOrgProduct
		{
			public string Name { get; set; }
			public decimal Price { get; set; }
		}

        [Test]
        public void TypedCombinedConditionsFromODataOrg()
        {
			var client = new ODataClient("http://services.odata.org/V2/OData/OData.svc/");
            var product = client
                .For<ODataOrgProduct>("Product")
                .Filter(x => x.Name == "Bread" && x.Price < 1000)
                .FindEntry();
            Assert.AreEqual(2.5m, product.Price);
        }
    }
}
