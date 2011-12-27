using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data;
using Simple.OData.Schema;
using Xunit;

namespace Simple.OData.Test
{
    public class CommandBuilderTest
    {
        private CommandBuilder _commandBuilder = new CommandBuilder(x => new Table(x));

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
