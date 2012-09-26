using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using NExtLib;
using NExtLib.Unit;

namespace Simple.NExtLib.Tests
{
    
    public class StringExtensionTests
    {
        [Fact]
        public void EnsureStartsWith_should_prefix_string()
        {
            var actual = "bar".EnsureStartsWith("foo");

            actual.ShouldEqual("foobar");
        }

        [Fact]
        public void EnsureStartsWith_should_not_prefix_string()
        {
            var actual = "foobar".EnsureStartsWith("foo");

            actual.ShouldEqual("foobar");
        }
    }
}
