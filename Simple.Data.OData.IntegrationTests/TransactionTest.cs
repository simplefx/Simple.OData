using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.OData.IntegrationTests
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

        [Fact]
        public void InsertUpdateSameEntitySingleTrans()
        {
            using (var tx = _db.BeginTransaction())
            {
                tx.Products.Insert(ProductID: 1001, ProductName: "Test1", UnitPrice: 21m);
                tx.Products.UpdateByProductID(ProductID: 1001, ProductName: "Test2");
                tx.Commit();
            }

            var product = _db.Products.FindByProductID(1001);
            Assert.Equal("Test1", product.ProductName);
        }

        [Fact]
        public void InsertUpdateDiffEntitiesSingleTrans()
        {
            using (var tx = _db.BeginTransaction())
            {
                tx.Products.Insert(ProductID: 1001, ProductName: "Test1", UnitPrice: 21m);
                tx.Commit();
            }
            using (var tx = _db.BeginTransaction())
            {
                tx.Products.Insert(ProductID: 1002, ProductName: "Test2", UnitPrice: 22m);
                tx.Products.UpdateByProductID(ProductID: 1001, ProductName: "Test3");
                tx.Commit();
            }

            var product = _db.Products.FindByProductID(1001);
            Assert.Equal("Test3", product.ProductName);
        }

        [Fact]
        public void InsertUpdateDeleteSameEntitySingleTrans()
        {
            using (var tx = _db.BeginTransaction())
            {
                tx.Products.Insert(ProductID: 1001, ProductName: "Test1", UnitPrice: 21m);
                tx.Products.UpdateByProductID(ProductID: 1001, ProductName: "Test2");
                tx.Products.Delete(ProductID: 1001);
                tx.Commit();
            }

            var product = _db.Products.FindByProductID(1001);
            Assert.Equal("Test1", product.ProductName);
        }

        [Fact]
        public void InsertUpdateDeleteDiffEntitiesSingleTrans()
        {
            using (var tx = _db.BeginTransaction())
            {
                tx.Products.Insert(ProductID: 1001, ProductName: "Test1", UnitPrice: 21m);
                tx.Products.Insert(ProductID: 1002, ProductName: "Test2", UnitPrice: 22m);
                tx.Commit();
            }
            using (var tx = _db.BeginTransaction())
            {
                tx.Products.Insert(ProductID: 1003, ProductName: "Test3", UnitPrice: 23m);
                tx.Products.UpdateByProductID(ProductID: 1001, ProductName: "Test4");
                tx.Products.Delete(ProductID: 1002);
                tx.Commit();
            }

            var product = _db.Products.FindByProductID(1001);
            Assert.Equal("Test4", product.ProductName);
            product = _db.Products.FindByProductID(1002);
            Assert.Null(product);
            product = _db.Products.FindByProductID(1003);
            Assert.Equal("Test3", product.ProductName);
        }

        [Fact]
        public void InsertUpdateDeleteSeparateTrans()
        {
            using (var tx = _db.BeginTransaction())
            {
                tx.Products.Insert(ProductID: 1001, ProductName: "Test1", UnitPrice: 21m);
                tx.Commit();
            }
            var product = _db.Products.FindByProductID(1001);
            Assert.Equal("Test1", product.ProductName);

            using (var tx = _db.BeginTransaction())
            {
                tx.Products.UpdateByProductID(ProductID: 1001, ProductName: "Test2");
                tx.Commit();
            }
            product = _db.Products.FindByProductID(1001);
            Assert.Equal("Test2", product.ProductName);

            using (var tx = _db.BeginTransaction())
            {
                tx.Products.Delete(ProductID: 1001);
                tx.Commit();
            }
            product = _db.Products.FindByProductID(1001);
            Assert.Null(product);
        }
    }
}
