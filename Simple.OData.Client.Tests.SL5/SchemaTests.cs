using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Simple.OData.Client.Tests
{
    [TestClass]
    public class SchemaTests
    {
        [TestMethod]
        public async Task CheckODataOrgNorthwindSchema()
        {
            //var client = new ODataClient("http://services.odata.org/V2/Northwind/Northwind.svc/");
            //var schema = await client.GetSchemaAsync();

            //var table = schema.FindEntitySet("Product");
            ////Assert.AreEqual("ProductID", table.PrimaryKey[0]);

            ////var association = EntitySet.FindAssociation("Categories");
            ////Assert.AreEqual("Categories", association.ReferenceTableName);
            ////Assert.AreEqual("0..1", association.Multiplicity);

            //table = schema.FindEntitySet("Employees");
            ////association = EntitySet.FindAssociation("Employees");
            ////Assert.AreEqual("Employees", association.ReferenceTableName);
            ////Assert.AreEqual("0..1", association.Multiplicity);

            //Assert.AreEqual(26, schema.EntityTypes.Count());
            //Assert.AreEqual(0, schema.ComplexTypes.Count());
        }

        [TestMethod]
        public async Task CheckODataOrgODataV2Schema()
        {
            //var client = new ODataClient("http://services.odata.org/V2/OData/OData.svc/");
            //var schema = await client.GetSchemaAsync();

            //var table = schema.FindEntitySet("Product");
            ////Assert.AreEqual("ID", table.PrimaryKey[0]);

            ////var association = EntitySet.FindAssociation("Category_Products");
            ////Assert.AreEqual("Categories", association.ReferenceTableName);
            ////Assert.AreEqual("0..1", association.Multiplicity);

            ////var function = schema.FindFunction("GetProductsByRating");
            ////Assert.AreEqual("rating", function.Parameters[0]);

            //Assert.AreEqual(3, schema.EntityTypes.Count());
            //Assert.AreEqual(1, schema.ComplexTypes.Count());
            //Assert.AreEqual(5, schema.ComplexTypes.First().Properties.Count());
        }

        [TestMethod]
        public async Task CheckODataOrgODataV3Schema()
        {
            //var client = new ODataClient("http://services.odata.org/V3/OData/OData.svc/");
            //var schema = await client.GetSchemaAsync();

            //var table = schema.FindEntitySet("Product");
            ////Assert.AreEqual("ID", table.PrimaryKey[0]);

            ////var association = EntitySet.FindAssociation("Category_Products");
            ////Assert.AreEqual("Categories", association.ReferenceTableName);
            ////Assert.AreEqual("*", association.Multiplicity);

            ////var function = schema.FindFunction("GetProductsByRating");
            ////Assert.AreEqual("rating", function.Parameters[0]);

            //Assert.AreEqual(10, schema.EntityTypes.Count());
            //Assert.AreEqual(1, schema.ComplexTypes.Count());
            //Assert.AreEqual(5, schema.ComplexTypes.First().Properties.Count());
        }
    }
}
