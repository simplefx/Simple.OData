
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server Compact Edition
-- --------------------------------------------------
-- Date Created: 05/15/2014 22:06:38
-- Generated from EDMX file: D:\Projects\Git\Simple.OData.Client\Simple.OData.NorthwindModel\Northwind.edmx
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- NOTE: if the constraint does not exist, an ignorable error will be reported.
-- --------------------------------------------------

    ALTER TABLE [Order Details] DROP CONSTRAINT [FK_Order_Details_FK00];
GO
    ALTER TABLE [Order Details] DROP CONSTRAINT [FK_Order_Details_FK01];
GO
    ALTER TABLE [Orders] DROP CONSTRAINT [FK_Orders_FK00];
GO
    ALTER TABLE [Orders] DROP CONSTRAINT [FK_Orders_FK01];
GO
    ALTER TABLE [Orders] DROP CONSTRAINT [FK_Orders_FK02];
GO
    ALTER TABLE [Products] DROP CONSTRAINT [FK_Products_FK00];
GO
    ALTER TABLE [Products] DROP CONSTRAINT [FK_Products_FK01];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- NOTE: if the table does not exist, an ignorable error will be reported.
-- --------------------------------------------------

    DROP TABLE [Categories];
GO
    DROP TABLE [Customers];
GO
    DROP TABLE [Employees];
GO
    DROP TABLE [Order Details];
GO
    DROP TABLE [Orders];
GO
    DROP TABLE [Products];
GO
    DROP TABLE [Shippers];
GO
    DROP TABLE [Suppliers];
GO
    DROP TABLE [Transport_Ships];
GO
    DROP TABLE [Transport_Trucks];
GO
    DROP TABLE [Transport];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Categories'
CREATE TABLE [Categories] (
    [CategoryID] int IDENTITY(1,1) NOT NULL,
    [CategoryName] nvarchar(15)  NOT NULL,
    [Description] ntext  NULL,
    [Picture] image  NULL
);
GO

-- Creating table 'Customers'
CREATE TABLE [Customers] (
    [CustomerID] nvarchar(5)  NOT NULL,
    [CompanyName] nvarchar(40)  NOT NULL,
    [ContactName] nvarchar(30)  NULL,
    [ContactTitle] nvarchar(30)  NULL,
    [Address] nvarchar(60)  NULL,
    [City] nvarchar(15)  NULL,
    [Region] nvarchar(15)  NULL,
    [PostalCode] nvarchar(10)  NULL,
    [Country] nvarchar(15)  NULL,
    [Phone] nvarchar(24)  NULL,
    [Fax] nvarchar(24)  NULL
);
GO

-- Creating table 'Employees'
CREATE TABLE [Employees] (
    [EmployeeID] int IDENTITY(1,1) NOT NULL,
    [LastName] nvarchar(20)  NOT NULL,
    [FirstName] nvarchar(10)  NOT NULL,
    [Title] nvarchar(30)  NULL,
    [BirthDate] datetime  NULL,
    [HireDate] datetime  NULL,
    [Address] nvarchar(60)  NULL,
    [City] nvarchar(15)  NULL,
    [Region] nvarchar(15)  NULL,
    [PostalCode] nvarchar(10)  NULL,
    [Country] nvarchar(15)  NULL,
    [HomePhone] nvarchar(24)  NULL,
    [Extension] nvarchar(4)  NULL,
    [Photo] image  NULL,
    [Notes] ntext  NULL,
    [ReportsTo] int  NULL
);
GO

-- Creating table 'Order_Details'
CREATE TABLE [Order Details] (
    [OrderID] int  NOT NULL,
    [ProductID] int  NOT NULL,
    [UnitPrice] decimal(19,4)  NOT NULL,
    [Quantity] smallint  NOT NULL,
    [Discount] real  NOT NULL
);
GO

-- Creating table 'Orders'
CREATE TABLE [Orders] (
    [OrderID] int  NOT NULL,
    [CustomerID] nvarchar(5)  NOT NULL,
    [EmployeeID] int  NULL,
    [ShipName] nvarchar(40)  NULL,
    [ShipAddress] nvarchar(60)  NULL,
    [ShipCity] nvarchar(15)  NULL,
    [ShipRegion] nvarchar(15)  NULL,
    [ShipPostalCode] nvarchar(10)  NULL,
    [ShipCountry] nvarchar(15)  NULL,
    [ShipVia] int  NULL,
    [OrderDate] datetime  NULL,
    [RequiredDate] datetime  NULL,
    [ShippedDate] datetime  NULL,
    [Freight] decimal(19,4)  NULL
);
GO

-- Creating table 'Products'
CREATE TABLE [Products] (
    [ProductID] int IDENTITY(1,1) NOT NULL,
    [SupplierID] int  NULL,
    [CategoryID] int  NULL,
    [ProductName] nvarchar(40)  NOT NULL,
    [EnglishName] nvarchar(40)  NULL,
    [QuantityPerUnit] nvarchar(20)  NULL,
    [UnitPrice] decimal(19,4)  NULL,
    [UnitsInStock] smallint  NULL,
    [UnitsOnOrder] smallint  NULL,
    [ReorderLevel] smallint  NULL,
    [Discontinued] bit  NOT NULL
);
GO

-- Creating table 'Shippers'
CREATE TABLE [Shippers] (
    [ShipperID] int IDENTITY(1,1) NOT NULL,
    [CompanyName] nvarchar(40)  NOT NULL
);
GO

-- Creating table 'Suppliers'
CREATE TABLE [Suppliers] (
    [SupplierID] int IDENTITY(1,1) NOT NULL,
    [CompanyName] nvarchar(40)  NOT NULL,
    [ContactName] nvarchar(30)  NULL,
    [ContactTitle] nvarchar(30)  NULL,
    [Address] nvarchar(60)  NULL,
    [City] nvarchar(15)  NULL,
    [Region] nvarchar(15)  NULL,
    [PostalCode] nvarchar(10)  NULL,
    [Country] nvarchar(15)  NULL,
    [Phone] nvarchar(24)  NULL,
    [Fax] nvarchar(24)  NULL
);
GO

-- Creating table 'Transport'
CREATE TABLE [Transport] (
    [TransportID] int IDENTITY(1,1) NOT NULL
);
GO

-- Creating table 'Transport_Ships'
CREATE TABLE [Transport_Ships] (
    [ShipName] nvarchar(100)  NULL,
    [TransportID] int  NOT NULL
);
GO

-- Creating table 'Transport_Trucks'
CREATE TABLE [Transport_Trucks] (
    [TruckNumber] nvarchar(100)  NULL,
    [TransportID] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [CategoryID] in table 'Categories'
ALTER TABLE [Categories]
ADD CONSTRAINT [PK_Categories]
    PRIMARY KEY ([CategoryID] );
GO

-- Creating primary key on [CustomerID] in table 'Customers'
ALTER TABLE [Customers]
ADD CONSTRAINT [PK_Customers]
    PRIMARY KEY ([CustomerID] );
GO

-- Creating primary key on [EmployeeID] in table 'Employees'
ALTER TABLE [Employees]
ADD CONSTRAINT [PK_Employees]
    PRIMARY KEY ([EmployeeID] );
GO

-- Creating primary key on [OrderID], [ProductID] in table 'Order_Details'
ALTER TABLE [Order Details]
ADD CONSTRAINT [PK_Order_Details]
    PRIMARY KEY ([OrderID], [ProductID] );
GO

-- Creating primary key on [OrderID] in table 'Orders'
ALTER TABLE [Orders]
ADD CONSTRAINT [PK_Orders]
    PRIMARY KEY ([OrderID] );
GO

-- Creating primary key on [ProductID] in table 'Products'
ALTER TABLE [Products]
ADD CONSTRAINT [PK_Products]
    PRIMARY KEY ([ProductID] );
GO

-- Creating primary key on [ShipperID] in table 'Shippers'
ALTER TABLE [Shippers]
ADD CONSTRAINT [PK_Shippers]
    PRIMARY KEY ([ShipperID] );
GO

-- Creating primary key on [SupplierID] in table 'Suppliers'
ALTER TABLE [Suppliers]
ADD CONSTRAINT [PK_Suppliers]
    PRIMARY KEY ([SupplierID] );
GO

-- Creating primary key on [TransportID] in table 'Transport'
ALTER TABLE [Transport]
ADD CONSTRAINT [PK_Transport]
    PRIMARY KEY ([TransportID] );
GO

-- Creating primary key on [TransportID] in table 'Transport_Ships'
ALTER TABLE [Transport_Ships]
ADD CONSTRAINT [PK_Transport_Ships]
    PRIMARY KEY ([TransportID] );
GO

-- Creating primary key on [TransportID] in table 'Transport_Trucks'
ALTER TABLE [Transport_Trucks]
ADD CONSTRAINT [PK_Transport_Trucks]
    PRIMARY KEY ([TransportID] );
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [CategoryID] in table 'Products'
ALTER TABLE [Products]
ADD CONSTRAINT [FK_Products_FK01]
    FOREIGN KEY ([CategoryID])
    REFERENCES [Categories]
        ([CategoryID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Products_FK01'
CREATE INDEX [IX_FK_Products_FK01]
ON [Products]
    ([CategoryID]);
GO

-- Creating foreign key on [CustomerID] in table 'Orders'
ALTER TABLE [Orders]
ADD CONSTRAINT [FK_Orders_FK00]
    FOREIGN KEY ([CustomerID])
    REFERENCES [Customers]
        ([CustomerID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Orders_FK00'
CREATE INDEX [IX_FK_Orders_FK00]
ON [Orders]
    ([CustomerID]);
GO

-- Creating foreign key on [EmployeeID] in table 'Orders'
ALTER TABLE [Orders]
ADD CONSTRAINT [FK_Orders_FK02]
    FOREIGN KEY ([EmployeeID])
    REFERENCES [Employees]
        ([EmployeeID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Orders_FK02'
CREATE INDEX [IX_FK_Orders_FK02]
ON [Orders]
    ([EmployeeID]);
GO

-- Creating foreign key on [ProductID] in table 'Order_Details'
ALTER TABLE [Order Details]
ADD CONSTRAINT [FK_Order_Details_FK00]
    FOREIGN KEY ([ProductID])
    REFERENCES [Products]
        ([ProductID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Order_Details_FK00'
CREATE INDEX [IX_FK_Order_Details_FK00]
ON [Order Details]
    ([ProductID]);
GO

-- Creating foreign key on [OrderID] in table 'Order_Details'
ALTER TABLE [Order Details]
ADD CONSTRAINT [FK_Order_Details_FK01]
    FOREIGN KEY ([OrderID])
    REFERENCES [Orders]
        ([OrderID])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [ShipVia] in table 'Orders'
ALTER TABLE [Orders]
ADD CONSTRAINT [FK_Orders_FK01]
    FOREIGN KEY ([ShipVia])
    REFERENCES [Shippers]
        ([ShipperID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Orders_FK01'
CREATE INDEX [IX_FK_Orders_FK01]
ON [Orders]
    ([ShipVia]);
GO

-- Creating foreign key on [SupplierID] in table 'Products'
ALTER TABLE [Products]
ADD CONSTRAINT [FK_Products_FK00]
    FOREIGN KEY ([SupplierID])
    REFERENCES [Suppliers]
        ([SupplierID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Products_FK00'
CREATE INDEX [IX_FK_Products_FK00]
ON [Products]
    ([SupplierID]);
GO

-- Creating foreign key on [ReportsTo] in table 'Employees'
ALTER TABLE [Employees]
ADD CONSTRAINT [FK_Employees_FK00]
    FOREIGN KEY ([ReportsTo])
    REFERENCES [Employees]
        ([EmployeeID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Employees_FK00'
CREATE INDEX [IX_FK_Employees_FK00]
ON [Employees]
    ([ReportsTo]);
GO

-- Creating foreign key on [TransportID] in table 'Transport_Ships'
ALTER TABLE [Transport_Ships]
ADD CONSTRAINT [FK_Ships_inherits_Transport]
    FOREIGN KEY ([TransportID])
    REFERENCES [Transport]
        ([TransportID])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [TransportID] in table 'Transport_Trucks'
ALTER TABLE [Transport_Trucks]
ADD CONSTRAINT [FK_Trucks_inherits_Transport]
    FOREIGN KEY ([TransportID])
    REFERENCES [Transport]
        ([TransportID])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------