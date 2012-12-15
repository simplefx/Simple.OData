using System;
using Xunit;

namespace Simple.OData.Client.Tests
{
    public class FilterAsKeyTests : TestBase
    {
        [Fact]
        public void FindAllByProductID()
        {
            var x = ODataFilter.Expression;
            var command = _client
                .From("Products")
                .Filter(x.ProductID == 1);
            string commandText = command.CommandText;
            Assert.Equal("Products(1)", commandText);
        }

        [Fact]
        public void FindAllEmployeeSuperiors()
        {
            var x = ODataFilter.Expression;
            var command = _client
                .From("Employees")
                .Filter(x.EmployeeID == 1)
                .NavigateTo("Superior");
            string commandText = command.CommandText;
            Assert.Equal("Employees(1)/Superior", commandText);
        }

        [Fact]
        public void FindAllCustomerOrders()
        {
            var x = ODataFilter.Expression;
            var command = _client
                .From("Customers")
                .Filter(x.CustomerID == "ALFKI")
                .NavigateTo("Orders");
            string commandText = command.CommandText;
            Assert.Equal("Customers('ALFKI')/Orders", commandText);
        }

        [Fact]
        public void FindAllEmployeeSubordinates()
        {
            var x = ODataFilter.Expression;
            var command = _client
                .From("Employees")
                .Filter(x.EmployeeID == 2)
                .NavigateTo("Subordinates");
            string commandText = command.CommandText;
            Assert.Equal("Employees(2)/Subordinates", commandText);
        }

        [Fact]
        public void FindAllOrderOrderDetails()
        {
            var x = ODataFilter.Expression;
            var command = _client
                .From("Orders")
                .Filter(x.OrderID == 10952)
                .NavigateTo("OrderDetails");
            string commandText = command.CommandText;
            Assert.Equal("Orders(10952)/Order_Details", commandText);
        }

        [Fact]
        public void FindEmployeeSuperior()
        {
            var x = ODataFilter.Expression;
            var command = _client
                .From("Employees")
                .Filter(x.EmployeeID == 1)
                .NavigateTo("Superior");
            string commandText = command.CommandText;
            Assert.Equal("Employees(1)/Superior", commandText);
        }
    }
}