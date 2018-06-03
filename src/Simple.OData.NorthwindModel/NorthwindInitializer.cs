using System;
using System.Data.Entity;
using NorthwindModel;
using Simple.OData.NorthwindModel.Entities;

namespace Simple.OData.NorthwindModel
{
    public class NorthwindInitializer : DropCreateDatabaseAlways<NorthwindContext>
    {
        protected override void Seed(NorthwindContext ctx)
        {
            ctx.Categories.AddRange(new[]
            {
                new Category { CategoryID = 1, CategoryName = "Beverages", Description = "Beverages Soft drinks, coffees, teas, beer, and ale", Picture = new byte[] {1,2,3} },
                new Category { CategoryID = 2, CategoryName = "Condiments", Description = "Sweet and savory sauces, relishes, spreads, and seasonings", Picture = new byte[] {1,2,3}  },
                new Category { CategoryID = 3, CategoryName = "Confections", Description = "Desserts, candies, sweetbreads", Picture = new byte[] {1,2,3}  },
                new Category { CategoryID = 4, CategoryName = "Dairy Products", Description = "Cheeses", Picture = new byte[] {1,2,3}  },
                new Category { CategoryID = 5, CategoryName = "Grains/Cereals", Description = "Breads, crackers, pasta, and cereal", Picture = new byte[] {1,2,3}  },
                new Category { CategoryID = 6, CategoryName = "Meat/Poultry", Description = "Prepared meats", Picture = new byte[] {1,2,3}  },
                new Category { CategoryID = 7, CategoryName = "Produce", Description = "Dried fruit and bean curd", Picture = new byte[] {1,2,3}  },
                new Category { CategoryID = 8, CategoryName = "Seafood", Description = "Seaweed and fish", Picture = new byte[] {1,2,3}  },
            });

            ctx.Suppliers.AddRange(new[]
            {
                new Supplier { SupplierID = 1, CompanyName = "Exotic Liquids", ContactName = "Charlotte Cooper", ContactTitle = "Purchasing Manager", Address = "49 Gilbert St.", City = "London", Region = null, PostalCode = "EC1 4SD", Country = "UK", Phone = "(71) 555-2222", Fax = null },
                new Supplier { SupplierID = 2, CompanyName = "New Orleans Cajun Delights", ContactName = "Shelley Burke", ContactTitle = "Order Administrator", Address = "P.O. Box 78934", City = "New Orleans", Region = "LA", PostalCode = "70117", Country = "USA", Phone = "(100) 555-4822", Fax = null },
                new Supplier { SupplierID = 3, CompanyName = "Grandma Kelly's Homestead", ContactName = "Regina Murphy", ContactTitle = "Sales Representative", Address = "707 Oxford Rd.", City = "Ann Arbor", Region = "MI", PostalCode = "48104", Country = "USA", Phone = "(313) 555-5735", Fax = "(313) 555-3349" },
                new Supplier { SupplierID = 4, CompanyName = "Tokyo Traders", ContactName = "Yoshi Nagase", ContactTitle = "Marketing Manager", Address = "9-8 Sekimai Musashino-shi", City = "Tokyo", Region = null, PostalCode = "100", Country = "Japan", Phone = "(03) 3555-5011", Fax = null },
                new Supplier { SupplierID = 5, CompanyName = "Cooperativa de Quesos 'Las Cabras'", ContactName = "Antonio del Valle Saavedra ", ContactTitle = "Export Administrator", Address = "Calle del Rosal 4", City = "Oviedo", Region = "Asturias", PostalCode = "33007", Country = "Spain", Phone = "(98) 598 76 54", Fax = null },
                new Supplier { SupplierID = 6, CompanyName = "Mayumi's", ContactName = "Mayumi Ohno", ContactTitle = "Marketing Representative", Address = "92 Setsuko Chuo-ku", City = "Osaka", Region = null, PostalCode = "545", Country = "Japan", Phone = "(06) 431-7877", Fax = null },
                new Supplier { SupplierID = 7, CompanyName = "Pavlova, Ltd.", ContactName = "Ian Devling", ContactTitle = "Marketing Manager", Address = "74 Rose St. Moonie Ponds", City = "Melbourne", Region = "Victoria", PostalCode = "3058", Country = "Australia", Phone = "(03) 444-2343", Fax = "(03) 444-6588" },
                new Supplier { SupplierID = 12, CompanyName = "Plusspar Lebensmittelgroßmärkte AG", ContactName = "Martin Bein", ContactTitle = "International Marketing Mgr.", Address = "Bogenallee 51", City = "Frankfurt", Region = null, PostalCode = "60439", Country = "Germany", Phone = "(069) 992755", Fax = null },
                new Supplier { SupplierID = 14, CompanyName = "Formaggi Fortini s.r.l.", ContactName = "Elio Rossi", ContactTitle = "Sales Representative", Address = "Viale Dante, 75", City = "Ravenna", Region = null, PostalCode = "48100", Country = "Italy", Phone = "(0544) 60323", Fax = "(0544) 60603" },
                new Supplier { SupplierID = 19, CompanyName = "New England Seafood Cannery", ContactName = "Robb Merchant", ContactTitle = "Wholesale Account Agent", Address = "Order Processing Dept. 2100 Paul Revere Blvd.", City = "Boston", Region = "MA", PostalCode = "02134", Country = "USA", Phone = "(503) 555-9931", Fax = null },
                new Supplier { SupplierID = 20, CompanyName = "Leka Trading", ContactName = "Chandra Leka", ContactTitle = "Owner", Address = "471 Serangoon Loop, Suite #402", City = "Singapore", Region = null, PostalCode = "0512", Country = "Singapore", Phone = "555-8787", Fax = null },
                new Supplier { SupplierID = 22, CompanyName = "Zaanse Snoepfabriek", ContactName = "Dirk Luchte", ContactTitle = "Accounting Manager", Address = "Verkoop Rijnweg 22", City = "Zaandam", Region = null, PostalCode = "9999 ZZ", Country = "Netherlands", Phone = "(12345) 1212", Fax = "(12345) 1210" },
                //new Supplier { SupplierID = 0, CompanyName = "", ContactName = "", ContactTitle = "", Address = "", City = "", Region = null, PostalCode = "", Country = "", Phone = "", Fax = null },
            });

            ctx.Products.AddRange(new[]
            {
                new Product { ProductID = 1, ProductName = "Chai", SupplierID = 1, CategoryID = 1, QuantityPerUnit = "10 boxes x 20 bags", UnitPrice = 18, UnitsInStock = 39, UnitsOnOrder = 0, ReorderLevel = 10, Discontinued = false },
                new Product { ProductID = 2, ProductName = "Chang", SupplierID = 1, CategoryID = 1, QuantityPerUnit = "24 - 12 oz bottles", UnitPrice = 19, UnitsInStock = 17, UnitsOnOrder = 40, ReorderLevel = 25, Discontinued = false },
                new Product { ProductID = 3, ProductName = "Aniseed Syrup", SupplierID = 1, CategoryID = 2, QuantityPerUnit = "12 - 550 ml bottles", UnitPrice = 10, UnitsInStock = 13, UnitsOnOrder = 70, ReorderLevel = 25, Discontinued = false },
                new Product { ProductID = 4, ProductName = "Chef Anton's Cajun Seasoning", SupplierID = 2, CategoryID = 2, QuantityPerUnit = "48 - 6 oz jars", UnitPrice = 22, UnitsInStock = 53, UnitsOnOrder = 0, ReorderLevel = 0, Discontinued = false },
                new Product { ProductID = 5, ProductName = "Chef Anton's Gumbo Mix", SupplierID = 2, CategoryID = 2, QuantityPerUnit = "36 boxes", UnitPrice = 21.35m, UnitsInStock = 0, UnitsOnOrder = 0, ReorderLevel = 0, Discontinued = true }, 
                new Product { ProductID = 6, ProductName = "Grandma's Boysenberry Spread", SupplierID = 3, CategoryID = 2, QuantityPerUnit = "12 - 8 oz jars", UnitPrice = 25, UnitsInStock = 120, UnitsOnOrder = 0, ReorderLevel = 25, Discontinued = false },
                new Product { ProductID = 7, ProductName = "Uncle Bob's Organic Dried Pears", SupplierID = 3, CategoryID = 7, QuantityPerUnit = "12 - 1 lb pkgs.", UnitPrice = 30, UnitsInStock = 15, UnitsOnOrder = 0, ReorderLevel = 10, Discontinued = false },
                new Product { ProductID = 8, ProductName = "Northwoods Cranberry Sauce", SupplierID = 3, CategoryID = 2, QuantityPerUnit = "12 - 12 oz jars", UnitPrice = 40, UnitsInStock = 6, UnitsOnOrder = 0, ReorderLevel = 0, Discontinued = false },
                new Product { ProductID = 9, ProductName = "Mishi Kobe Niku", SupplierID = 4, CategoryID = 6, QuantityPerUnit = "18 - 500 g pkgs.", UnitPrice = 97, UnitsInStock = 29, UnitsOnOrder = 0, ReorderLevel = 0, Discontinued = true }, 
                new Product { ProductID = 10, ProductName = "Ikura", SupplierID = 4, CategoryID = 8, QuantityPerUnit = "12 - 200 ml jars", UnitPrice = 31, UnitsInStock = 31, UnitsOnOrder = 0, ReorderLevel = 0, Discontinued = false }, 
                new Product { ProductID = 11, ProductName = "Queso Cabrales", SupplierID = 5, CategoryID = 4, QuantityPerUnit = "1 kg pkg.", UnitPrice = 21, UnitsInStock = 22, UnitsOnOrder = 30, ReorderLevel = 30, Discontinued = false }, 
                new Product { ProductID = 12, ProductName = "Queso Manchego La Pastora", SupplierID = 5, CategoryID = 4, QuantityPerUnit = "10 - 500 g pkgs.", UnitPrice = 38, UnitsInStock = 86, UnitsOnOrder = 0, ReorderLevel = 0, Discontinued = false }, 
                new Product { ProductID = 13, ProductName = "Konbu", SupplierID = 6, CategoryID = 8, QuantityPerUnit = "2 kg box", UnitPrice = 6, UnitsInStock = 0, UnitsOnOrder = 24, ReorderLevel = 5, Discontinued = false }, 
                new Product { ProductID = 14, ProductName = "Tofu", SupplierID = 6, CategoryID = 7, QuantityPerUnit = "40 - 100 g pkgs.", UnitPrice = 23.25m, UnitsInStock = 35, UnitsOnOrder = 0, ReorderLevel = 0, Discontinued = false }, 
                new Product { ProductID = 15, ProductName = "Genen Shouyu", SupplierID = 6, CategoryID = 2, QuantityPerUnit = "24 - 250 ml bottles", UnitPrice = 15.5m, UnitsInStock = 39, UnitsOnOrder = 0, ReorderLevel = 5, Discontinued = false }, 
                new Product { ProductID = 16, ProductName = "Pavlova", SupplierID = 7, CategoryID = 3, QuantityPerUnit = "32 - 500 g boxes", UnitPrice = 17.45m, UnitsInStock = 29, UnitsOnOrder = 0, ReorderLevel = 10, Discontinued = false }, 
                new Product { ProductID = 17, ProductName = "Alice Mutton", SupplierID = 7, CategoryID = 6, QuantityPerUnit = "20 - 1 kg tins", UnitPrice = 39, UnitsInStock = 0, UnitsOnOrder = 0, ReorderLevel = 0, Discontinued = true }, 
                new Product { ProductID = 40, ProductName = "Boston Crab Meat", SupplierID = 19, CategoryID = 8, QuantityPerUnit = "24 - 4 oz tins", UnitPrice = 18.4m, UnitsInStock = 123, UnitsOnOrder = 0, ReorderLevel = 30, Discontinued = false }, 
                new Product { ProductID = 42, ProductName = "Singaporean Hokkien Fried Mee", SupplierID = 20, CategoryID = 5, QuantityPerUnit = "32 - 1 kg pkgs.", UnitPrice = 14, UnitsInStock = 26, UnitsOnOrder = 0, ReorderLevel = 0, Discontinued = true }, 
                new Product { ProductID = 47, ProductName = "Zaanse koeken", SupplierID = 22, CategoryID = 3, QuantityPerUnit = "10 - 4 oz boxes", UnitPrice = 9.5m, UnitsInStock = 36, UnitsOnOrder = 0, ReorderLevel = 0, Discontinued = false }, 
                new Product { ProductID = 72, ProductName = "Mozzarella di Giovanni", SupplierID = 14, CategoryID = 4, QuantityPerUnit = "24 - 200 g pkgs.", UnitPrice = 34.8m, UnitsInStock = 14, UnitsOnOrder = 0, ReorderLevel = 0, Discontinued = false }, 
                new Product { ProductID = 77, ProductName = "Original Frankfurter grüne Soße", SupplierID = 12, CategoryID = 2, QuantityPerUnit = "12 boxes", UnitPrice = 13, UnitsInStock = 32, UnitsOnOrder = 0, ReorderLevel = 15, Discontinued = false }, 
                //new Product { ProductID = 0, ProductName = "", SupplierID = 0, CategoryID = 0, QuantityPerUnit = "", UnitPrice = 0, UnitsInStock = 0, UnitsOnOrder = 0, ReorderLevel = 0, Discontinued = false }, 
            });

            ctx.Customers.AddRange(new[]
            {
                new Customer { CustomerID = "BOTTM", CompanyName = "Bottom-Dollar Markets", ContactName = "Elizabeth Lincoln", ContactTitle = "Accounting Manager", Address = "23 Tsawassen Blvd.", City = "Tsawassen", Region = "BC", PostalCode = "T2F 8M4", Country = "Canada", Phone = "(604) 555-4729", Fax = "(604) 555-3745" },
                new Customer { CustomerID = "ERNSH", CompanyName = "Ernst Handel", ContactName = "Roland Mendel", ContactTitle = "Sales Manager", Address = "Kirchgasse 6", City = "Graz", Region = null, PostalCode = "8010", Country = "Austria", Phone = "7675-3425", Fax = "7675-3426" },
                new Customer { CustomerID = "FRANS", CompanyName = "Franchi S.p.A.", ContactName = "Paolo Accorti", ContactTitle = "Sales Representative", Address = "Via Monte Bianco 34", City = "Torino", Region = null, PostalCode = "10100", Country = "Italy", Phone = "011-4988260", Fax = "011-4988261" },
                new Customer { CustomerID = "SAVEA", CompanyName = "Save-a-lot Markets", ContactName = "Jose Pavarotti", ContactTitle = "Sales Representative", Address = "187 Suffolk Ln.", City = "Boise", Region = "ID", PostalCode = "83720", Country = "USA", Phone = "(208) 555-8097", Fax = "" },
                new Customer { CustomerID = "VINET", CompanyName = "Vins et alcools Chevalier", ContactName = "Paul Henriot", ContactTitle = "Accounting Manager", Address = "59 rue de l'Abbaye", City = "Reims", Region = null, PostalCode = "51100", Country = "France", Phone = "26.47.15.10", Fax = "26.47.15.11" },
                // new Customer { CustomerID = "", CompanyName = "", ContactName = "", ContactTitle = "", Address = "", City = "", Region = null, PostalCode = "", Country = "", Phone = "", Fax = "" },
            });

            ctx.Employees.AddRange(new[]
            {
                new Employee { EmployeeID = 1, LastName = "Davolio", FirstName = "Nancy", Title = "Sales Representative", BirthDate = new DateTime(1948,12,8), HireDate = new DateTime(1991,3,29), Address = "507 - 20th Ave. E. Apt. 2A", City = "Seattle", Region = "WA", PostalCode = "98122", Country = "USA", HomePhone = "(206) 555-9857", Extension = "5467", Photo = new byte[] {1,2,3}, Notes = "Notes", ReportsTo = 2 },
                new Employee { EmployeeID = 2, LastName = "Fuller", FirstName = "Andrew", Title = "Vice President, Sales", BirthDate = new DateTime(1942,2,19), HireDate = new DateTime(1991,7,12), Address = "", City = "", Region = null, PostalCode = "", Country = "", HomePhone = "", Extension = "", Photo = new byte[] {1,2,3}, Notes = null, ReportsTo = null },
                new Employee { EmployeeID = 3, LastName = "Leverling", FirstName = "Janet", Title = "Sales Representative", BirthDate = new DateTime(1963,8,30), HireDate = new DateTime(1991,2,27), Address = "", City = "", Region = null, PostalCode = "", Country = "", HomePhone = "", Extension = "", Photo = new byte[] {1,2,3}, Notes = null, ReportsTo = 2 },
                new Employee { EmployeeID = 4, LastName = "Peacock", FirstName = "Margaret", Title = "Sales Representative", BirthDate = new DateTime(1937,9,19), HireDate = new DateTime(1992,3,30), Address = "", City = "", Region = null, PostalCode = "", Country = "", HomePhone = "", Extension = "", Photo = new byte[] {1,2,3}, Notes = null, ReportsTo = 2 },
                new Employee { EmployeeID = 5, LastName = "Buchanan", FirstName = "Steven", Title = "Sales Manager", BirthDate = new DateTime(1955,3,4), HireDate = new DateTime(1992,9,13), Address = "", City = "", Region = null, PostalCode = "", Country = "", HomePhone = "", Extension = "", Photo = new byte[] {1,2,3}, Notes = null, ReportsTo = 2 },
                new Employee { EmployeeID = 6, LastName = "Suyama", FirstName = "Michael", Title = "Sales Representative", BirthDate = new DateTime(1963,7,2), HireDate = new DateTime(1992,9,13), Address = "", City = "", Region = null, PostalCode = "", Country = "", HomePhone = "", Extension = "", Photo = new byte[] {1,2,3}, Notes = null, ReportsTo = 5 },
                new Employee { EmployeeID = 7, LastName = "King", FirstName = "Robert", Title = "Sales Representative", BirthDate = new DateTime(1960,5,29), HireDate = new DateTime(1992,11,29), Address = "", City = "", Region = null, PostalCode = "", Country = "", HomePhone = "", Extension = "", Photo = new byte[] {1,2,3}, Notes = null, ReportsTo = 5 },
                new Employee { EmployeeID = 13, LastName = "Brid", FirstName = "Justin", Title = "Marketing Director", BirthDate = new DateTime(1962,10,8), HireDate = new DateTime(1994,1,1), Address = "", City = "", Region = null, PostalCode = "", Country = "", HomePhone = "", Extension = "", Photo = new byte[] {1,2,3}, Notes = null, ReportsTo = 2 },
                new Employee { EmployeeID = 14, LastName = "Martin", FirstName = "Xavier", Title = "Marketing Associate", BirthDate = new DateTime(1960,11,30), HireDate = new DateTime(1994,1,15), Address = "", City = "", Region = null, PostalCode = "", Country = "", HomePhone = "", Extension = "", Photo = new byte[] {1,2,3}, Notes = null, ReportsTo = 13 },
                //new Employee { EmployeeID = 0, LastName = "", FirstName = "", Title = "", BirthDate = DateTime.MinValue, HireDate = DateTime.MinValue, Address = "", City = "", Region = null, PostalCode = "", Country = "", HomePhone = "", Extension = "", Photo = new byte[] {1,2,3}, Notes = null, ReportsTo = 0 },
            });

            ctx.Shippers.AddRange(new[]
            {
                new Shipper { ShipperID = 1, CompanyName = "Speedy Express" },
                new Shipper { ShipperID = 2, CompanyName = "United Package" },
                new Shipper { ShipperID = 3, CompanyName = "Federal Shipping" },
            });

            ctx.Orders.AddRange(new[]
            {
                new Order { OrderID = 10000, CustomerID = "FRANS", EmployeeID = 6, ShipName = "Franchi S.p.A.", ShipAddress = "Via Monte Bianco 34", ShipCity = "Torino", ShipRegion = null, ShipPostalCode = "10100", ShipCountry = "Italy", ShipVia = 3, OrderDate = new DateTime(1991,10,5), RequiredDate = new DateTime(1991,6,7), ShippedDate = new DateTime(1991,5,15), Freight = 4.45m },
                new Order { OrderID = 10021, CustomerID = "ERNSH", EmployeeID = 7, ShipName = "Ernst Handel", ShipAddress = "Kirchgasse 6", ShipCity = "Graz", ShipRegion = null, ShipPostalCode = "8010", ShipCountry = "Austria", ShipVia = 3, OrderDate = new DateTime(1991,6,14), RequiredDate = new DateTime(1991,7,12), ShippedDate = new DateTime(1991,7,2), Freight = 75.17m },
                new Order { OrderID = 10065, CustomerID = "SAVEA", EmployeeID = 1, ShipName = "Save-a-lot Markets", ShipAddress = "187 Suffolk Ln.", ShipCity = "Boise", ShipRegion = "ID", ShipPostalCode = "83720", ShipCountry = "USA", ShipVia = 3, OrderDate = new DateTime(1991,8,28), RequiredDate = new DateTime(1991,9,25), ShippedDate = new DateTime(1991,9,3), Freight = 45.03m },
                new Order { OrderID = 10199, CustomerID = "SAVEA", EmployeeID = 1, ShipName = "Save-a-lot Markets", ShipAddress = "187 Suffolk Ln.", ShipCity = "Boise", ShipRegion = "ID", ShipPostalCode = "83720", ShipCountry = "USA", ShipVia = 3, OrderDate = new DateTime(1992,3,27), RequiredDate = new DateTime(1992,4,10), ShippedDate = new DateTime(1992,3,30), Freight = 50.19m },
                new Order { OrderID = 10248, CustomerID = "VINET", EmployeeID = 5, ShipName = "Vins et alcools Chevalier", ShipAddress = "59 rue de l'Abbaye", ShipCity = "Reims", ShipRegion = null, ShipPostalCode = "51100", ShipCountry = "France", ShipVia = 3, OrderDate = new DateTime(1992,5,28), RequiredDate = new DateTime(1992,6,25), ShippedDate = new DateTime(1992,6,9), Freight = 32.38m },
                new Order { OrderID = 10847, CustomerID = "SAVEA", EmployeeID = 4, ShipName = "Save-a-lot Markets", ShipAddress = "187 Suffolk Ln.", ShipCity = "Boise", ShipRegion = "ID", ShipPostalCode = "83720", ShipCountry = "USA", ShipVia = 3, OrderDate = new DateTime(1993,12,16), RequiredDate = new DateTime(1993,12,30), ShippedDate = new DateTime(1994,1,4), Freight = 487.57m },
                new Order { OrderID = 10918, CustomerID = "BOTTM", EmployeeID = 3, ShipName = "Bottom-Dollar Markets", ShipAddress = "23 Tsawassen Blvd.", ShipCity = "Tsawassen", ShipRegion = "BC", ShipPostalCode = "T2F 8M4", ShipCountry = "Canada", ShipVia = 3, OrderDate = new DateTime(1994,1,24), RequiredDate = new DateTime(1994,2,21), ShippedDate = new DateTime(1994,2,2), Freight = 48.83m },
                // new Order { OrderID = 0, CustomerID = "", EmployeeID = , ShipName = "", ShipAddress = "", ShipCity = "", ShipRegion = null, ShipPostalCode = "", ShipCountry = "", ShipVia = 0, OrderDate = new DateTime(0,0,0), RequiredDate = new DateTime(0,0,0), ShippedDate = new DateTime(0,0,0), Freight = 0m },
            });

            ctx.Order_Details.AddRange(new[]
            {
                new OrderDetail { OrderID = 10021, ProductID = 1, UnitPrice = 12m, Quantity = 60, Discount = 0.2f },
                new OrderDetail { OrderID = 10065, ProductID = 1, UnitPrice = 12.6m, Quantity = 55, Discount = 0.25f },
                new OrderDetail { OrderID = 10199, ProductID = 1, UnitPrice = 12m, Quantity = 66, Discount = 0 },
                new OrderDetail { OrderID = 10248, ProductID = 11, UnitPrice = 14, Quantity = 12, Discount = 0 },
                new OrderDetail { OrderID = 10248, ProductID = 42, UnitPrice = 9.8m, Quantity = 10, Discount = 0 },
                new OrderDetail { OrderID = 10248, ProductID = 72, UnitPrice = 24.3m, Quantity = 5, Discount = 0 },
                new OrderDetail { OrderID = 10847, ProductID = 1, UnitPrice = 18m, Quantity = 80, Discount = 0.2f },
                new OrderDetail { OrderID = 10918, ProductID = 1, UnitPrice = 18m, Quantity = 60, Discount = 0.25f },
                // new OrderDetail { OrderID = 0, ProductID = 0, UnitPrice = 0m, Quantity = 0, Discount = 0 },
            });

            ctx.Transport.AddRange(new Transport[]
            {
                new Ship { TransportID = 1, TransportType = 1, ShipName = "Titanic" },
                new Truck { TransportID = 2, TransportType = 2, TruckNumber = "123456" },
            });
        }
    }
}
