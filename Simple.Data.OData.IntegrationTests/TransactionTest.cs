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
                tx.Products.Insert(ProductName: "Test1", UnitPrice: 21m);
                tx.Commit();
            }

            var product = _db.Products.FindByProductName("Test1");
            Assert.True(product.ProductID > 0);
        }

        [Fact]
        public void InsertOneTransRollback()
        {
            using (var tx = _db.BeginTransaction())
            {
                tx.Products.Insert(ProductName: "Test2", UnitPrice: 21m);
                tx.Rollback();
            }

            var product = _db.Products.FindByProductName("Test2");
            Assert.Null(product);
        }

        [Fact]
        public void InsertTwoTransCommit()
        {
            using (var tx = _db.BeginTransaction())
            {
                tx.Products.Insert(ProductName: "Test3", UnitPrice: 21m);
                tx.Products.Insert(ProductName: "Test4", UnitPrice: 22m);
                tx.Commit();
            }

            var product = _db.Products.FindByProductName("Test3");
            Assert.Equal(21m, product.UnitPrice);
            product = _db.Products.FindByProductName("Test4");
            Assert.Equal(22m, product.UnitPrice);
        }

        [Fact]
        public void InsertTwoTransRollback()
        {
            using (var tx = _db.BeginTransaction())
            {
                tx.Products.Insert(ProductName: "Test5", UnitPrice: 21m);
                tx.Products.Insert(ProductName: "Test6", UnitPrice: 22m);
                tx.Rollback();
            }

            var product = _db.Products.FindByProductName("Test5");
            Assert.Null(product);
            product = _db.Products.FindByProductName("Test6");
            Assert.Null(product);
        }

        [Fact]
        public void InsertUpdateSameEntitySingleTrans()
        {
            using (var tx = _db.BeginTransaction())
            {
                tx.Products.Insert(ProductName: "Test7", UnitPrice: 21m);
                tx.Products.UpdateByProductName(ProductName: "Test7", UnitPrice: 22m);
                tx.Commit();
            }

            var product = _db.Products.FindByProductName("Test7");
            Assert.Equal(21m, product.UnitPrice);
        }

        [Fact]
        public void InsertUpdateDiffEntitiesSingleTrans()
        {
            using (var tx = _db.BeginTransaction())
            {
                tx.Products.Insert(ProductName: "Test8", UnitPrice: 21m);
                tx.Commit();
            }
            using (var tx = _db.BeginTransaction())
            {
                tx.Products.Insert(ProductName: "Test9", UnitPrice: 22m);
                tx.Products.UpdateByProductName(ProductName: "Test8", UnitPrice: 23m);
                tx.Commit();
            }

            var product = _db.Products.FindByProductName("Test8");
            Assert.Equal(23m, product.UnitPrice);
        }

        [Fact]
        public void InsertUpdateDeleteSameEntitySingleTrans()
        {
            using (var tx = _db.BeginTransaction())
            {
                tx.Products.Insert(ProductName: "Test10", UnitPrice: 21m);
                tx.Products.UpdateByProductName(ProductName: "Test10", UnitPrice: 22m);
                tx.Products.Delete(ProductName: "Test10");
                tx.Commit();
            }

            var product = _db.Products.FindByProductName("Test10");
            Assert.Equal(21m, product.UnitPrice);
        }

        [Fact]
        public void InsertUpdateDeleteDiffEntitiesSingleTrans()
        {
            using (var tx = _db.BeginTransaction())
            {
                tx.Products.Insert(ProductName: "Test11", UnitPrice: 21m);
                tx.Products.Insert(ProductName: "Test12", UnitPrice: 22m);
                tx.Commit();
            }
            using (var tx = _db.BeginTransaction())
            {
                tx.Products.Insert(ProductName: "Test13", UnitPrice: 23m);
                tx.Products.UpdateByProductName(ProductName: "Test11", UnitPrice: 24m);
                tx.Products.Delete(ProductName: "Test12");
                tx.Commit();
            }

            var product = _db.Products.FindByProductName("Test11");
            Assert.Equal(24m, product.UnitPrice);
            product = _db.Products.FindByProductName("Test12");
            Assert.Null(product);
            product = _db.Products.FindByProductName("Test13");
            Assert.Equal(23m, product.UnitPrice);
        }

        [Fact]
        public void InsertUpdateDeleteSeparateTrans()
        {
            using (var tx = _db.BeginTransaction())
            {
                tx.Products.Insert(ProductName: "Test14", UnitPrice: 21m);
                tx.Commit();
            }
            var product = _db.Products.FindByProductName("Test14");
            Assert.Equal(21m, product.UnitPrice);

            using (var tx = _db.BeginTransaction())
            {
                tx.Products.UpdateByProductName(ProductName: "Test14", UnitPrice: 22m);
                tx.Commit();
            }
            product = _db.Products.FindByProductName("Test14");
            Assert.Equal(22m, product.UnitPrice);

            using (var tx = _db.BeginTransaction())
            {
                tx.Products.Delete(ProductName: "Test14");
                tx.Commit();
            }
            product = _db.Products.FindByProductName("Test14");
            Assert.Null(product);
        }

        [Fact]
        public void InsertSingleEntityWithAssociationSingleTrans()
        {
            dynamic category;
            using (var tx = _db.BeginTransaction())
            {
                category = tx.Categories.Insert(CategoryName: "Test15");
                tx.Products.Insert(ProductName: "Test16", UnitPrice: 18m, Category: category);
                tx.Commit();
            }

            category = _db.Categories.FindByCategoryName("Test15");
            var product = _db.Products.WithCategory().FindByProductName("Test16");
            Assert.Equal(category.CategoryName, product.Category.CategoryName);
        }
    }
}
