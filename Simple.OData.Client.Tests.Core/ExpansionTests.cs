using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace Simple.OData.Client.Tests
{
    public class ExpansionTests : TestBase
    {
        [Theory]
        [InlineData("Northwind.edmx", "Employees?$expand=Subordinates")]
        [InlineData("Northwind4.edmx", "Employees?$expand=Subordinates")]
        public async Task ExpandSubordinates(string metadataFile, string expectedCommand)
        {
            var client = CreateClient(metadataFile);
            var command = client
                .For<Employee>()
                .Expand(x => x.Subordinates);
            string commandText = await command.GetCommandTextAsync();
            Assert.Equal(expectedCommand, commandText);
        }

        [Theory]
        [InlineData("Northwind.edmx", "Employees?$expand=Subordinates,Superior")]
        [InlineData("Northwind4.edmx", "Employees?$expand=Subordinates,Superior")]
        public async Task ExpandSubordinatesAndSuperior(string metadataFile, string expectedCommand)
        {
            var client = CreateClient(metadataFile);
            var command = client
                .For<Employee>()
                .Expand(x => new { x.Subordinates, x.Superior });
            string commandText = await command.GetCommandTextAsync();
            Assert.Equal(expectedCommand, commandText);
        }

        [Theory]
        [InlineData("Northwind.edmx", "Employees?$expand=Subordinates,Superior")]
        [InlineData("Northwind4.edmx", "Employees?$expand=Subordinates,Superior")]
        public async Task ExpandSubordinatesAndSuperiorTwoClauses(string metadataFile, string expectedCommand)
        {
            var client = CreateClient(metadataFile);
            var command = client
                .For<Employee>()
                .Expand(x => x.Subordinates)
                .Expand(x => x.Superior);
            string commandText = await command.GetCommandTextAsync();
            Assert.Equal(expectedCommand, commandText);
        }

        [Theory]
        [InlineData("Northwind.edmx", "Employees?$expand=Subordinates/Subordinates")]
        [InlineData("Northwind4.edmx", "Employees?$expand=Subordinates($expand=Subordinates)")]
        public async Task ExpandSubordinatesTwoTimes(string metadataFile, string expectedCommand)
        {
            var client = CreateClient(metadataFile);
            var command = client
                .For<Employee>()
                .Expand(x => x.Subordinates.Select(y => y.Subordinates));
            string commandText = await command.GetCommandTextAsync();
            Assert.Equal(expectedCommand, commandText);
        }

        [Theory]
        [InlineData("Northwind.edmx", "Employees?$expand=Subordinates/Subordinates/Subordinates")]
        [InlineData("Northwind4.edmx", "Employees?$expand=Subordinates($expand=Subordinates($expand=Subordinates))")]
        public async Task ExpandSubordinatesThreeTimes(string metadataFile, string expectedCommand)
        {
            var client = CreateClient(metadataFile);
            var command = client
                .For<Employee>()
                .Expand(x => x.Subordinates.Select(y => y.Subordinates.Select(z => z.Subordinates)));
            string commandText = await command.GetCommandTextAsync();
            Assert.Equal(expectedCommand, commandText);
        }

        [Theory]
        [InlineData("Northwind.edmx", "Employees?$expand=Subordinates&$select=LastName,Subordinates&$orderby=LastName")]
        [InlineData("Northwind4.edmx", "Employees?$expand=Subordinates&$select=LastName,Subordinates&$orderby=LastName")]
        public async Task ExpandSubordinatesWithSelectAndOrderby(string metadataFile, string expectedCommand)
        {
            var client = CreateClient(metadataFile);
            var command = client
                .For<Employee>()
                .Expand(x => x.Subordinates)
                .Select(x => new { x.LastName, x.Subordinates })
                .OrderBy(x => x.LastName);
            string commandText = await command.GetCommandTextAsync();
            Assert.Equal(expectedCommand, commandText);
        }

        [Theory]
        [InlineData("Northwind.edmx", "Employees?$expand=Subordinates/Subordinates&$select=LastName,Subordinates,Subordinates/LastName,Subordinates/Subordinates&$orderby=LastName,Subordinates/LastName")]
        [InlineData("Northwind4.edmx", "Employees?$expand=Subordinates($expand=Subordinates;$select=LastName,Subordinates;$orderby=LastName)&$select=LastName,Subordinates&$orderby=LastName")]
        public async Task ExpandSubordinatesWithSelectAndOrderbyTwoTimes(string metadataFile, string expectedCommand)
        {
            var client = CreateClient(metadataFile);
            var command = client
                .For<Employee>()
                .Expand(x => x.Subordinates.Select(y => y.Subordinates))
                .Select(x => new { x.LastName, x.Subordinates })
                .Select(x => x.Subordinates.Select(y => new { y.LastName, y.Subordinates }))
                .OrderBy(x => x.LastName)
                .OrderBy(x => x.Subordinates.Select(y => y.LastName));
            string commandText = await command.GetCommandTextAsync();
            Assert.Equal(expectedCommand, commandText);
        }

        [Theory]
        [InlineData("Northwind.edmx", "Employees?$expand=Subordinates/Subordinates/Subordinates&$select=LastName,Subordinates,Subordinates/LastName,Subordinates/Subordinates,Subordinates/Subordinates/LastName,Subordinates/Subordinates/Subordinates&$orderby=LastName,Subordinates/LastName,Subordinates/Subordinates/LastName")]
        [InlineData("Northwind4.edmx", "Employees?$expand=Subordinates($expand=Subordinates($expand=Subordinates;$select=LastName,Subordinates;$orderby=LastName);$select=LastName,Subordinates;$orderby=LastName)&$select=LastName,Subordinates&$orderby=LastName")]
        public async Task ExpandSubordinatesWithSelectAndOrderbyThreeTimes(string metadataFile, string expectedCommand)
        {
            var client = CreateClient(metadataFile);
            var command = client
                .For<Employee>()
                .Expand(x => x.Subordinates.Select(y => y.Subordinates.Select(z => z.Subordinates)))
                .Select(x => new { x.LastName, x.Subordinates })
                .Select(x => x.Subordinates.Select(y => new { y.LastName, y.Subordinates }))
                .Select(x => x.Subordinates.Select(y => y.Subordinates.Select(z => new { z.LastName, z.Subordinates })))
                .OrderBy(x => x.LastName)
                .OrderBy(x => x.Subordinates.Select(y => y.LastName))
                .OrderBy(x => x.Subordinates.Select(y => y.Subordinates.Select(z => z.LastName)));
            string commandText = await command.GetCommandTextAsync();
            Assert.Equal(expectedCommand, commandText);
        }

        [Theory]
        [InlineData("Northwind4.edmx", "Employees?$expand=Subordinates($levels=2)")]
        public async Task ExpandSubordinates2Levels(string metadataFile, string expectedCommand)
        {
            var client = CreateClient(metadataFile);
            var command = client
                .For<Employee>()
                .Expand(ODataExpandOptions.ByValue(2), x => x.Subordinates);

            string commandText = await command.GetCommandTextAsync();
            Assert.Equal(expectedCommand, commandText);
        }
    }
}