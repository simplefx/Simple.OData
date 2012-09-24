using System;
using System.Collections.Generic;
using Simple.Data.OData.Schema;
using Xunit;

namespace Simple.Data.OData.UnitTests
{
    public class CommandBuilderTest
    {
        private CommandBuilder _commandBuilder = new CommandBuilder(x => new Table(x, null));

        [Fact]
        public void BuildsCommandFromTableNameWithNoFilter()
        {
            string command = _commandBuilder.BuildCommand("Products", null);
            Assert.Equal("Products", command);
        }

        [Fact]
        public void BuildsCommandFromTableNameWithFilter()
        {
            string command = _commandBuilder.BuildCommand("Products", "a eq 1");
            Assert.Equal("Products?$filter=a+eq+1", command);
        }
    }
}
