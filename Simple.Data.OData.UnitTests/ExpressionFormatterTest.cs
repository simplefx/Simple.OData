using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.OData.UnitTests
{
    using Xunit;
    using Simple.Data;

    public class ExpressionFormatterTest
    {
        private ExpressionFormatter _expressionFormatter = new ExpressionFormatter(null);

        [Fact]
        public void SingleEqualsFormatsAsODataFilter()
        {
            var expression = ObjectReference.FromString("foo.bar") == 1;
            var actual = _expressionFormatter.Format(expression);
            Assert.Equal("bar eq 1", actual);
        }

        [Fact]
        public void TwoEqualsFormatsAsODataFilter()
        {
            var expression = ObjectReference.FromString("foo.bar") == 1 && ObjectReference.FromString("foo.quux") == 2;
            var actual = _expressionFormatter.Format(expression);
            Assert.Equal("(bar eq 1 and quux eq 2)", actual);
        }

        [Fact]
        public void SingleNotEqualsFormatsAsODataFilter()
        {
            var expression = ObjectReference.FromString("foo.bar") != 1;
            var actual = _expressionFormatter.Format(expression);
            Assert.Equal("bar ne 1", actual);
        }

        [Fact]
        public void GreaterThanFormatsAsODataFilter()
        {
            var expression = ObjectReference.FromString("foo.bar") > 1;
            var actual = _expressionFormatter.Format(expression);
            Assert.Equal("bar gt 1", actual);
        }

        [Fact]
        public void GreaterThanOrEqualFormatsAsODataFilter()
        {
            var expression = ObjectReference.FromString("foo.bar") >= 1;
            var actual = _expressionFormatter.Format(expression);
            Assert.Equal("bar ge 1", actual);
        }

        [Fact]
        public void LessThanFormatsAsODataFilter()
        {
            var expression = ObjectReference.FromString("foo.bar") < 1;
            var actual = _expressionFormatter.Format(expression);
            Assert.Equal("bar lt 1", actual);
        }

        [Fact]
        public void LessThanOrEqualFormatsAsODataFilter()
        {
            var expression = ObjectReference.FromString("foo.bar") <= 1;
            var actual = _expressionFormatter.Format(expression);
            Assert.Equal("bar le 1", actual);
        }

        [Fact]
        public void EqualWithRangeFormatsAsODataFilter()
        {
            var expression = ObjectReference.FromString("foo.bar") == 1.to(10);
            var actual = _expressionFormatter.Format(expression);
            Assert.Equal("(bar ge 1 and bar le 10)", actual);
        }

        [Fact]
        public void EqualsWithAddFormatsAsODataFilter()
        {
            var expression = ObjectReference.FromString("bar") + ObjectReference.FromString("1") == 2;
            var actual = _expressionFormatter.Format(expression);
            Assert.Equal("bar add 1 eq 2", actual);
        }

        [Fact]
        public void EqualsWithSubFormatsAsODataFilter()
        {
            var expression = ObjectReference.FromString("bar") - ObjectReference.FromString("1") == 2;
            var actual = _expressionFormatter.Format(expression);
            Assert.Equal("bar sub 1 eq 2", actual);
        }

        [Fact]
        public void EqualsWithMulFormatsAsODataFilter()
        {
            var expression = ObjectReference.FromString("bar") * ObjectReference.FromString("1") == 2;
            var actual = _expressionFormatter.Format(expression);
            Assert.Equal("bar mul 1 eq 2", actual);
        }

        [Fact]
        public void EqualsWithDivFormatsAsODataFilter()
        {
            var expression = ObjectReference.FromString("bar") / ObjectReference.FromString("1") == 2;
            var actual = _expressionFormatter.Format(expression);
            Assert.Equal("bar div 1 eq 2", actual);
        }

        [Fact]
        public void EqualsWithModFormatsAsODataFilter()
        {
            var expression = ObjectReference.FromString("bar") % ObjectReference.FromString("1") == 2;
            var actual = _expressionFormatter.Format(expression);
            Assert.Equal("bar mod 1 eq 2", actual);
        }

        [Fact]
        public void LengthFormatsAsODataFilter()
        {
            var expression = new SimpleExpression(new SimpleFunction("length", new object[] {ObjectReference.FromString("bar")}),
                                     1,
                                     SimpleExpressionType.Equal);
            var actual = _expressionFormatter.Format(expression);
            Assert.Equal("length(bar) eq 1", actual);
        }

        [Fact]
        public void SubstringOfFormatsAsODataFilter()
        {
            var expression =
                new SimpleExpression(
                    new SimpleFunction("substringof",
                                       new object[]
                                           {ObjectReference.FromString("bar"), ObjectReference.FromString("'abc'")}),
                    true,
                    SimpleExpressionType.Equal);
            var actual = _expressionFormatter.Format(expression);
            Assert.Equal("substringof(bar,'abc') eq true", actual);
        }

        [Fact]
        public void IndexOfFormatsAsODataFilter()
        {
            var expression =
                new SimpleExpression(
                    new SimpleFunction("indexof",
                                       new object[] { ObjectReference.FromString("bar"), ObjectReference.FromString("'abc'") }),
                    10,
                    SimpleExpressionType.Equal);
            var actual = _expressionFormatter.Format(expression);
            Assert.Equal("indexof(bar,'abc') eq 10", actual);
        }
    }
}
