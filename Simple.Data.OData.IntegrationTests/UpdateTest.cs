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
        public void Update()
        {
            _db.Products.UpdateByProductName(ProductName: "Chai", UnitPrice: 123m);
            var product = _db.Products.FindByProductName("Chai");

            Assert.Equal(123m, product.UnitPrice);
        }

        [Fact]
        public void UpdateAssociation()
        {
            var category = _db.Categories.Insert(CategoryID: 1001, CategoryName: "Test");
            _db.Products.UpdateByProductName(ProductName: "Chai", Category: category);
            var product = _db.Products.FindByProductName("Chai");

            Assert.Equal(1001, product.CategoryID);
            category = _db.Category.WithProducts().FindByCategoryName("Test");
            Assert.True(category.Products.Count == 1);
        }
    }
}
