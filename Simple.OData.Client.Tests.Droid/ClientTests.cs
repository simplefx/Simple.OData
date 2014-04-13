using System.Linq;
using NUnit.Framework;

namespace Simple.OData.Client.Tests
{
	[TestFixture]
    public class ClientTests
    {
        [Test]
        public async void CheckODataOrgNorthwindSchema()
        {
            var client = new ODataClient("http://services.odata.org/V2/Northwind/Northwind.svc/");

            var schema = await client.GetSchemaAsync();
            var table = schema.FindTable("Product");
            Assert.AreEqual("ProductID", table.PrimaryKey[0]);

            var association = table.FindAssociation("Categories");
            Assert.AreEqual("Categories", association.ReferenceTableName);
            Assert.AreEqual("0..1", association.Multiplicity);

            table = client.GetSchema().FindTable("Employees");
            association = table.FindAssociation("Employees");
            Assert.AreEqual("Employees", association.ReferenceTableName);
            Assert.AreEqual("0..1", association.Multiplicity);

            Assert.AreEqual(26, client.GetSchema().EntityTypes.Count());
            Assert.AreEqual(0, client.GetSchema().ComplexTypes.Count());
        }

        [Test]
        public async void CheckODataOrgODataSchema()
        {
            var client = new ODataClient("http://services.odata.org/V3/OData/OData.svc/");

            var schema = await client.GetSchemaAsync();
            var table = schema.FindTable("Product");
            Assert.AreEqual("ID", table.PrimaryKey[0]);

            var association = table.FindAssociation("Category_Products");
            Assert.AreEqual("Categories", association.ReferenceTableName);
            Assert.AreEqual("*", association.Multiplicity);

            var function = client.GetSchema().FindFunction("GetProductsByRating");
            Assert.AreEqual(RestVerbs.GET, function.HttpMethod);
            Assert.AreEqual("rating", function.Parameters[0]);

            Assert.AreEqual(10, client.GetSchema().EntityTypes.Count());
            Assert.AreEqual(1, client.GetSchema().ComplexTypes.Count());
            Assert.AreEqual(5, client.GetSchema().ComplexTypes.First().Properties.Count());
        }

        [Test]
        public async void AllEntriesFromODataOrg()
        {
            var client = new ODataClient("http://services.odata.org/V3/OData/OData.svc/");
            var products = await client
                .For("Product")
                .FindEntriesAsync();
            Assert.IsNotNull(products);
            Assert.AreNotEqual(0, products.Count());
        }

        [Test]
        public async void DynamicCombinedConditionsFromODataOrg()
        {
            var client = new ODataClient("http://services.odata.org/V2/OData/OData.svc/");
            var x = ODataDynamic.Expression;
            var product = await client
                .For(x.Product)
                .Filter(x.Name == "Bread" && x.Price < 1000)
                .FindEntryAsync();
            Assert.AreEqual(2.5m, product.Price);
        }

        public class ODataOrgProduct
        {
            public string Name { get; set; }
            public decimal Price { get; set; }
        }

        [Test]
        public async void TypedCombinedConditionsFromODataOrg()
        {
            var client = new ODataClient("http://services.odata.org/V2/OData/OData.svc/");
            var product = await client
                .For<ODataOrgProduct>("Product")
                .Filter(x => x.Name == "Bread" && x.Price < 1000)
                .FindEntryAsync();
            Assert.AreEqual(2.5m, product.Price);
        }
    }
}
