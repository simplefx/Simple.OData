using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.OData.IntegrationTest
{
    using Xunit;

    public class TransactionTest : TestBase
    {
        [Fact]
        public void InsertOneTransCommit()
        {
            using (var tx = _db.BeginTransaction())
            {
                tx.Products.Insert(ProductID: 1001, ProductName: "Test1", UnitPrice: 21m);
                tx.Commit();
            }

            var product = _db.Products.FindByProductID(1001);
            Assert.Equal("Test1", product.ProductName);
        }

        [Fact]
        public void InsertOneTransRollback()
        {
            using (var tx = _db.BeginTransaction())
            {
                tx.Products.Insert(ProductID: 1001, ProductName: "Test1", UnitPrice: 21m);
                tx.Rollback();
            }

            var product = _db.Products.FindByProductID(1001);
            Assert.Null(product);
        }

        [Fact]
        public void InsertTwoTransCommit()
        {
            using (var tx = _db.BeginTransaction())
            {
                tx.Products.Insert(ProductID: 1001, ProductName: "Test1", UnitPrice: 21m);
                tx.Products.Insert(ProductID: 1002, ProductName: "Test2", UnitPrice: 22m);
                tx.Commit();
            }

            var product = _db.Products.FindByProductID(1001);
            Assert.Equal("Test1", product.ProductName);
            product = _db.Products.FindByProductID(1002);
            Assert.Equal("Test2", product.ProductName);
        }

        [Fact]
        public void InsertTwoTransRollback()
        {
            using (var tx = _db.BeginTransaction())
            {
                tx.Products.Insert(ProductID: 1001, ProductName: "Test1", UnitPrice: 21m);
                tx.Products.Insert(ProductID: 1002, ProductName: "Test2", UnitPrice: 22m);
                tx.Rollback();
            }

            var product = _db.Products.FindByProductID(1001);
            Assert.Null(product);
            product = _db.Products.FindByProductID(1002);
            Assert.Null(product);
        }
    }
}
