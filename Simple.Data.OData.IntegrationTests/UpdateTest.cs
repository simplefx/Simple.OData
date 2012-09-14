using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.OData.IntegrationTests
{
    using Xunit;

    public class UpdateTest : TestBase
    {
        [Fact]
        public void UpdateSingleField()
        {
            _db.Products.UpdateByProductName(ProductName: "Chai", UnitPrice: 123m);
            var product = _db.Products.FindByProductName("Chai");

            Assert.Equal(123m, product.UnitPrice);
        }

        [Fact]
        public void UpdateWholeRecord()
        {
            var product = _db.Products.FindByProductID(1);
            product.UnitPrice = 123m;
            _db.Products.Update(product);
            product = _db.Products.FindByProductID(1);

            Assert.Equal(123m, product.UnitPrice);
        }

        [Fact]
        public void UpdateAssociation()
        {
            var category = _db.Categories.Insert(CategoryName: "Test1");
            var product = _db.Products.Insert(ProductName: "Test2", UnitPrice: 18m, CategoryID : 1);
            _db.Products.UpdateByProductName(ProductName: "Test2", Category: category);
            product = _db.Products.FindByProductName("Test2");

            Assert.Equal(category.CategoryID, product.CategoryID);
            category = _db.Category.WithProducts().FindByCategoryName("Test1");
            Assert.True(category.Products.Count == 1);
        }
    }
}
