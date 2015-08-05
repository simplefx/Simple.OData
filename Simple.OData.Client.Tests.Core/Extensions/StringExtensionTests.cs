using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.OData.Client.Extensions;
using Xunit;

namespace Simple.OData.Client.Tests
{
    public class StringExtensionTests
    {
        [Fact]
        public void EnsureStartsWith_should_prefix_string()
        {
            var actual = "bar".EnsureStartsWith("foo");

            Assert.Equal("foobar", actual);
        }

        [Fact]
        public void EnsureStartsWith_should_not_prefix_string()
        {
            var actual = "foobar".EnsureStartsWith("foo");

            Assert.Equal("foobar", actual);
        }
    }
}
