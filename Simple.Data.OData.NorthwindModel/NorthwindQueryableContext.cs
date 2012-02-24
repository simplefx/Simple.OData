using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Simple.Data.OData.NorthwindModel.Entities;

namespace Simple.Data.OData.NorthwindModel
{
    public sealed partial class NorthwindContext
    {
        static readonly NorthwindContext instance = new NorthwindContext();

        static NorthwindContext()
        {
        }

        NorthwindContext()
        {
        }

        public static NorthwindContext Instance
        {
            get
            {
                return instance;
            }
        }

        private ICollection<Categories> categories = new List<Categories>();
        private ICollection<Customers> customers = new List<Customers>();
        private ICollection<CustomerDemographics> customerDemographics = new List<CustomerDemographics>();
        private ICollection<Employees> employees = new List<Employees>();
        private ICollection<Orders> orders = new List<Orders>();
        private ICollection<OrderDetails> orderDetails = new List<OrderDetails>();
        private ICollection<Products> products = new List<Products>();
        private ICollection<Regions> regions = new List<Regions>();
        private ICollection<Shippers> shippers = new List<Shippers>();
        private ICollection<Suppliers> suppliers = new List<Suppliers>();
        private ICollection<Territories> territories = new List<Territories>();

        public IQueryable<Categories> Categories { get { return this.categories.AsQueryable(); } }
        public IQueryable<Customers> Customers { get { return this.customers.AsQueryable(); } }
        public IQueryable<CustomerDemographics> CustomerDemographics { get { return this.customerDemographics.AsQueryable(); } }
        public IQueryable<Employees> Employees { get { return this.employees.AsQueryable(); } }
        public IQueryable<Orders> Orders { get { return this.orders.AsQueryable(); } }
        public IQueryable<OrderDetails> OrderDetails { get { return this.orderDetails.AsQueryable(); } }
        public IQueryable<Products> Products { get { return this.products.AsQueryable(); } }
        public IQueryable<Regions> Regions { get { return this.regions.AsQueryable(); } }
        public IQueryable<Shippers> Shippers { get { return this.shippers.AsQueryable(); } }
        public IQueryable<Suppliers> Suppliers { get { return this.suppliers.AsQueryable(); } }
        public IQueryable<Territories> Territories { get { return this.territories.AsQueryable(); } }

        internal Categories SetProductCategory(Products product, int categoryID)
        {
            var category = this.categories.Where(x => x.CategoryID == product.CategoryID).SingleOrDefault();
            if (category != null)
                category.Products.Remove(product);
            category = this.categories.Where(x => x.CategoryID == categoryID).SingleOrDefault();
            if (category != null)
                category.Products.Add(product);
            return category;
        }

        internal Suppliers SetProductSupplier(Products product, int supplierID)
        {
            var supplier = this.suppliers.Where(x => x.SupplierID == product.SupplierID).SingleOrDefault();
            if (supplier != null)
                supplier.Products.Remove(product);
            supplier = this.suppliers.Where(x => x.SupplierID == supplierID).SingleOrDefault();
            if (supplier != null)
                supplier.Products.Add(product);
            return supplier;
        }

        internal Suppliers SetProductSupplier1(Products product, int supplierID)
        {
            return SetReference(product, supplierID, this.suppliers, x => x.SupplierID, x => x.SupplierID, x => x.Products);
        }

        internal T2 SetReference<T1, T2>(T1 entity, int referenceID, 
            ICollection<T2> collection, Func<T1, int> entityItemID, Func<T2, int> collectionItemID, Func<T2, ICollection<T1>> itemCollection)
        {
            var item = collection.Where(x => collectionItemID(x) == entityItemID(entity)).SingleOrDefault();
            if (item != null)
                itemCollection(item).Remove(entity);
            item = collection.Where(x => collectionItemID(x) == entityItemID(entity)).SingleOrDefault();
            if (item != null)
                itemCollection(item).Add(entity);
            return item;
        }
    }
}
