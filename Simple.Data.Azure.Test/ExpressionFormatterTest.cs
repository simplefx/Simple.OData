using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Azure.Test
{
    using Xunit;

    public class ExpressionFormatterTest
    {
        [Fact]
        public void SingleEqualsFormatsAsODataFilter()
        {
            var expression = ObjectReference.FromString("foo.bar") == 1;
            var actual = new ExpressionFormatter().Format(expression);
            Assert.Equal("bar eq 1", actual);
        }

        [Fact]
        public void TwoEqualsFormatsAsODataFilter()
        {
            var expression = ObjectReference.FromString("foo.bar") == 1 && ObjectReference.FromString("foo.quux") == 2;
            var actual = new ExpressionFormatter().Format(expression);
            Assert.Equal("(bar eq 1 and quux eq 2)", actual);
        }

        [Fact]
        public void SingleNotEqualsFormatsAsODataFilter()
        {
            var expression = ObjectReference.FromString("foo.bar") != 1;
            var actual = new ExpressionFormatter().Format(expression);
            Assert.Equal("bar ne 1", actual);
        }

        [Fact]
        public void GreaterThanFormatsAsODataFilter()
        {
            var expression = ObjectReference.FromString("foo.bar") > 1;
            var actual = new ExpressionFormatter().Format(expression);
            Assert.Equal("bar gt 1", actual);
        }

        [Fact]
        public void GreaterThanOrEqualFormatsAsODataFilter()
        {
            var expression = ObjectReference.FromString("foo.bar") >= 1;
            var actual = new ExpressionFormatter().Format(expression);
            Assert.Equal("bar ge 1", actual);
        }

        [Fact]
        public void LessThanFormatsAsODataFilter()
        {
            var expression = ObjectReference.FromString("foo.bar") < 1;
            var actual = new ExpressionFormatter().Format(expression);
            Assert.Equal("bar lt 1", actual);
        }

        [Fact]
        public void LessThanOrEqualFormatsAsODataFilter()
        {
            var expression = ObjectReference.FromString("foo.bar") <= 1;
            var actual = new ExpressionFormatter().Format(expression);
            Assert.Equal("bar le 1", actual);
        }

        [Fact]
        public void EqualWithRangeFormatsAsODataFilter()
        {
            var expression = ObjectReference.FromString("foo.bar") == 1.to(10);
            var actual = new ExpressionFormatter().Format(expression);
            Assert.Equal("(bar ge 1 and bar le 10)", actual);
        }
    }
}
