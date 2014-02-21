using System;
using Xunit;

namespace Simple.OData.Client.Tests
{
    public class TypedFilterAsKeyTests : TestBase
    {
        [Fact]
        public void FindAllByTypedFilterAsKeyEqual()
        {
            var command = _client
                .For<Product>()
                .Filter(x => x.ProductID == 1);
            string commandText = command.GetCommandText();
            Assert.Equal("Products(1)", commandText);
        }

        [Fact]
        public void FindAllByFilterAsKeyNotEqual()
        {
            var command = _client
                .For<Product>()
                .Filter(x => x.ProductID != 1);
            string commandText = command.GetCommandText();
            Assert.Equal("Products?$filter=ProductID%20ne%201", commandText);
        }

        [Fact]
        public void FindAllByFilterAsNotKeyEqual()
        {
            var command = _client
                .For<Product>()
                .Filter(x => !(x.ProductID == 1));
            string commandText = command.GetCommandText();
            Assert.Equal(string.Format("Products?$filter=not{0}ProductID%20eq%201{1}", 
                Uri.EscapeDataString("("), Uri.EscapeDataString(")")), commandText);
        }

        [Fact]
        public void FindAllByFilterAsKeyEqualLong()
        {
            var command = _client
                .For<Product>()
                .Filter(x => x.ProductID == 1L);
            string commandText = command.GetCommandText();
            Assert.Equal("Products(1L)", commandText);
        }

        [Fact]
        public void FindAllByFilterAsKeyEqualAndExtraClause()
        {
            var command = _client
                .For<Product>()
                .Filter(x => x.ProductID == 1 && x.ProductName == "abc");
            string commandText = command.GetCommandText();
            Assert.Equal(string.Format("Products?$filter=ProductID%20eq%201%20and%20ProductName%20eq%20{0}abc{0}", 
                Uri.EscapeDataString("'")), commandText);
        }

        [Fact]
        public void FindAllByFilterAsKeyEqualDuplicateClause()
        {
            var command = _client
                .For<Product>()
                .Filter(x => x.ProductID == 1 && x.ProductID == 1);
            string commandText = command.GetCommandText();
            Assert.Equal("Products(1)", commandText);
        }

        [Fact]
        public void FindAllByFilterAsCompleteCompoundKey()
        {
            var command = _client
                .For<OrderDetail>()
                .Filter(x => x.OrderID == 1 && x.ProductID == 2);
            string commandText = command.GetCommandText();
            Assert.Equal("Order_Details(OrderID=1,ProductID=2)", commandText);
        }

        [Fact]
        public void FindAllByFilterAsInCompleteCompoundKey()
        {
            var command = _client
                .For<OrderDetail>()
                .Filter(x => x.OrderID == 1);
            string commandText = command.GetCommandText();
            Assert.Equal("Order_Details?$filter=OrderID%20eq%201", commandText);
        }

        [Fact]
        public void FindAllEmployeeSuperiors()
        {
            var command = _client
                .For<Employee>()
                .Filter(x => x.EmployeeID == 1)
                .NavigateTo("Superior");
            string commandText = command.GetCommandText();
            Assert.Equal("Employees(1)/Superior", commandText);
        }

        [Fact]
        public void FindAllCustomerOrders()
        {
            var command = _client
                .For<Customer>()
                .Filter(x => x.CustomerID == "ALFKI")
                .NavigateTo<Order>();
            string commandText = command.GetCommandText();
            Assert.Equal("Customers('ALFKI')/Orders", commandText);
        }

        [Fact]
        public void FindAllEmployeeSubordinates()
        {
            var command = _client
                .For<Employee>()
                .Filter(x => x.EmployeeID == 2)
                .NavigateTo("Subordinates");
            string commandText = command.GetCommandText();
            Assert.Equal("Employees(2)/Subordinates", commandText);
        }

        [Fact]
        public void FindAllOrderOrderDetails()
        {
            var command = _client
                .For<Order>()
                .Filter(x => x.OrderID == 10952)
                .NavigateTo<OrderDetail>();
            string commandText = command.GetCommandText();
            Assert.Equal("Orders(10952)/Order_Details", commandText);
        }

        [Fact]
        public void FindEmployeeSuperior()
        {
            var command = _client
                .For<Employee>()
                .Filter(x => x.EmployeeID == 1)
                .NavigateTo("Superior");
            string commandText = command.GetCommandText();
            Assert.Equal("Employees(1)/Superior", commandText);
        }

        [Fact]
        public void FindAllFromBaseTableByFilterAsKeyEqual()
        {
            var command = _client
                .For<Transport>()
                .Filter(x => x.TransportID == 1);
            string commandText = command.GetCommandText();
            Assert.Equal("Transport(1)", commandText);
        }

        [Fact]
        public void FindAllFromDerivedTableByFilterAsKeyEqual()
        {
            var command = _client
                .For<Transport>()
                .As<Ship>()
                .Filter(x => x.TransportID == 1);
            string commandText = command.GetCommandText();
            Assert.Equal("Transport/NorthwindModel.Ships(1)", commandText);
        }
    }
}