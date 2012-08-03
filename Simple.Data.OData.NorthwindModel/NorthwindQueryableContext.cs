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

        internal Employees SetEmployeeSuperior(Employees employee, int? superiorID)
        {
            return SetReference(employee, superiorID, this.employees, x => x.Subordinates, x => x.ReportsTo, x => x.EmployeeID);
        }

        internal Orders SetOrderDetailsOrder(OrderDetails details, int orderID)
        {
            return SetReference(details, orderID, this.orders, x => x.OrderDetails, x => x.OrderID, x => x.OrderID);
        }

        internal Products SetOrderDetailsProduct(OrderDetails details, int productID)
        {
            return SetReference(details, productID, this.products, x => x.OrderDetails, x => x.ProductID, x => x.ProductID);
        }

        internal Customers SetOrderCustomer(Orders order, string customerID)
        {
            return SetReference(order, customerID, this.customers, x => x.Orders, x => x.CustomerID, x => x.CustomerID);
        }

        internal Employees SetOrderEmployee(Orders order, int employeeID)
        {
            return SetReference(order, employeeID, this.employees, x => x.Orders, x => x.EmployeeID, x => x.EmployeeID);
        }

        internal Shippers SetOrderShipper(Orders order, int shipperID)
        {
            return SetReference(order, shipperID, this.shippers, x => x.Orders, x => x.ShipVia, x => x.ShipperID);
        }

        internal Categories SetProductCategory(Products product, int categoryID)
        {
            return SetReference(product, categoryID, this.categories, x => x.Products, x => x.CategoryID, x => x.CategoryID);
        }

        internal Suppliers SetProductSupplier(Products product, int supplierID)
        {
            return SetReference(product, supplierID, this.suppliers, x => x.Products, x => x.SupplierID, x => x.SupplierID);
        }

        internal Regions SetTerritoryRegion(Territories territory, int regionID)
        {
            return SetReference(territory, regionID, this.regions, x => x.Territories, x => x.RegionID, x => x.RegionID);
        }

        internal T2 SetReference<T1, T2>(T1 entity, int referencedEntityID,
            ICollection<T2> contextCollection, Func<T2, ICollection<T1>> referencedCollectionFunc, 
            Func<T1, int> referencedEntityIDFunc, Func<T2, int> contextCollectionIDFunc)
        {
            lock (this)
            {
                var item = contextCollection.Where(x => contextCollectionIDFunc(x) == referencedEntityIDFunc(entity)).SingleOrDefault();
                if (item != null)
                    referencedCollectionFunc(item).Remove(entity);
                item = contextCollection.Where(x => contextCollectionIDFunc(x) == referencedEntityID).SingleOrDefault();
                if (item != null)
                    referencedCollectionFunc(item).Add(entity);
                return item;
            }
        }

        internal T2 SetReference<T1, T2>(T1 entity, int? referencedEntityID,
            ICollection<T2> contextCollection, Func<T2, ICollection<T1>> referencedCollectionFunc,
            Func<T1, int?> referencedEntityIDFunc, Func<T2, int?> contextCollectionIDFunc)
        {
            lock (this)
            {
                if (referencedEntityIDFunc(entity) != null)
                {
                    var item =
                        contextCollection.Where(x => contextCollectionIDFunc(x) == referencedEntityIDFunc(entity)).
                            SingleOrDefault();
                    if (item != null)
                        referencedCollectionFunc(item).Remove(entity);
                }
                if (referencedEntityID.HasValue)
                {
                    var item =
                        contextCollection.Where(x => contextCollectionIDFunc(x) == referencedEntityID.Value).
                            SingleOrDefault();
                    if (item != null)
                        referencedCollectionFunc(item).Add(entity);
                    return item;
                }
                else
                {
                    return default(T2);
                }
            }
        }

        internal T2 SetReference<T1, T2>(T1 entity, string referencedEntityID,
            ICollection<T2> contextCollection, Func<T2, ICollection<T1>> referencedCollectionFunc,
            Func<T1, string> referencedEntityIDFunc, Func<T2, string> contextCollectionIDFunc)
        {
            lock (this)
            {
                var item =
                    contextCollection.Where(x => contextCollectionIDFunc(x) == referencedEntityIDFunc(entity)).
                        SingleOrDefault();
                if (item != null)
                    referencedCollectionFunc(item).Remove(entity);
                item = contextCollection.Where(x => contextCollectionIDFunc(x) == referencedEntityID).SingleOrDefault();
                if (item != null)
                    referencedCollectionFunc(item).Add(entity);
                return item;
            }
        }
    }
}
