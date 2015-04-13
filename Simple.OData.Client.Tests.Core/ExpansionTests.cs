using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace Simple.OData.Client.Tests
{
    public class ExpansionTests : TestBase
    {
        [Fact]
        public async Task ExpandSubordinates()
        {
            var client = CreateClient("Northwind4.edmx");
            var command = client
                .For<Employee>()
                .Expand(x => x.Subordinates);
            string commandText = await command.GetCommandTextAsync();
            Assert.Equal("Employees?$expand=Subordinates", commandText);
        }

        [Fact]
        public async Task ExpandSubordinatesAndSuperior()
        {
            var client = CreateClient("Northwind4.edmx");
            var command = client
                .For<Employee>()
                .Expand(x => new { x.Subordinates, x.Superior });
            string commandText = await command.GetCommandTextAsync();
            Assert.Equal("Employees?$expand=Subordinates,Superior", commandText);
        }

        [Fact]
        public async Task ExpandSubordinatesAndSuperiorTwoClauses()
        {
            var client = CreateClient("Northwind4.edmx");
            var command = client
                .For<Employee>()
                .Expand(x => x.Subordinates)
                .Expand(x => x.Superior);
            string commandText = await command.GetCommandTextAsync();
            Assert.Equal("Employees?$expand=Subordinates,Superior", commandText);
        }

        [Fact]
        public async Task ExpandSubordinatesTwoTimes()
        {
            var client = CreateClient("Northwind4.edmx");
            var command = client
                .For<Employee>()
                .Expand(x => x.Subordinates.Select(y => y.Subordinates));
            string commandText = await command.GetCommandTextAsync();
            Assert.Equal("Employees?$expand=Subordinates($expand=Subordinates)", commandText);
        }

        [Fact]
        public async Task ExpandSubordinatesThreeTimes()
        {
            var client = CreateClient("Northwind4.edmx");
            var command = client
                .For<Employee>()
                .Expand(x => x.Subordinates.Select(y => y.Subordinates.Select(z => z.Subordinates)));
            string commandText = await command.GetCommandTextAsync();
            Assert.Equal("Employees?$expand=Subordinates($expand=Subordinates($expand=Subordinates))", commandText);
        }

        [Fact]
        public async Task ExpandSubordinatesWithSelectAndOrderbyThreeTimes()
        {
            var client = CreateClient("Northwind4.edmx");
            var command = client
                .For<Employee>()
                .Expand(x => x.Subordinates.Select(y => y.Subordinates.Select(z => z.Subordinates)))
                .Select(x => new {x.LastName, x.Subordinates})
                .Select(x => x.Subordinates.Select(y => new {y.LastName, y.Subordinates}))
                .Select(x => x.Subordinates.Select(y => y.Subordinates.Select(z => new {z.LastName, z.Subordinates})))
                .OrderBy(x => x.LastName)
                .OrderBy(x => x.Subordinates.Select(y => y.LastName))
                .OrderBy(x => x.Subordinates.Select(y => y.Subordinates.Select(z => z.LastName)));
            string commandText = await command.GetCommandTextAsync();
            Assert.Equal("Employees?$expand=Subordinates($expand=Subordinates($select=LastName,Subordinates;$orderby=LastName);$select=LastName,Subordinates;$orderby=LastName)&$select=LastName,Subordinates&orderby=LastName", commandText);
        }
    }
}