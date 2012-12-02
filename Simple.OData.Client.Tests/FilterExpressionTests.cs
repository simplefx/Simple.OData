using System;
using Xunit;

namespace Simple.OData.Client.Tests
{
    public class FilterExpressionTests
    {
        [Fact]
        public void And()
        {
            var x = ODataFilter.Context;
            string filter = x.CategoryID == 1 && x.ProductName == "Chai";
            Assert.Equal("CategoryID eq 1 and ProductName eq 'Chai'", filter);
        }

        [Fact]
        public void Or()
        {
            var x = ODataFilter.Context;
            string filter = x.ProductName == "Chai" || x.CategoryID == 1;
            Assert.Equal("ProductName eq 'Chai' or CategoryID eq 1", filter);
        }

        [Fact]
        public void Not()
        {
            var x = ODataFilter.Context;
            string filter = !(x.ProductName == "Chai");
            Assert.Equal("not(ProductName eq 'Chai')", filter);
        }

        [Fact]
        public void Precedence()
        {
            var x = ODataFilter.Context;
            string filter = (x.CategoryID == 1 || x.CategoryID == 2) && x.ProductName == "Chai";
            Assert.Equal("(CategoryID eq 1 or CategoryID eq 2) and ProductName eq 'Chai'", filter);
        }

        [Fact]
        public void EqualString()
        {
            var x = ODataFilter.Context;
            string filter = x.ProductName == "Chai";
            Assert.Equal("ProductName eq 'Chai'", filter);
        }

        [Fact]
        public void EqualNumeric()
        {
            var x = ODataFilter.Context;
            string filter = x.CategoryID == 1;
            Assert.Equal("CategoryID eq 1", filter);
        }

        [Fact]
        public void NotEqualNumeric()
        {
            var x = ODataFilter.Context;
            string filter = x.CategoryID != 1;
            Assert.Equal("CategoryID ne 1", filter);
        }

        [Fact]
        public void GreaterNumeric()
        {
            var x = ODataFilter.Context;
            string filter = x.CategoryID > 1;
            Assert.Equal("CategoryID gt 1", filter);
        }

        [Fact]
        public void GreaterOrEqualNumeric()
        {
            var x = ODataFilter.Context;
            string filter = x.CategoryID >= 1.5;
            Assert.Equal("CategoryID ge 1.5", filter);
        }

        [Fact]
        public void LessNumeric()
        {
            var x = ODataFilter.Context;
            string filter = x.CategoryID < 1;
            Assert.Equal("CategoryID lt 1", filter);
        }

        [Fact]
        public void LessOrEqualNumeric()
        {
            var x = ODataFilter.Context;
            string filter = x.CategoryID <= 1;
            Assert.Equal("CategoryID le 1", filter);
        }

        [Fact]
        public void AddEqualNumeric()
        {
            var x = ODataFilter.Context;
            string filter = x.CategoryID + 1 == 2;
            Assert.Equal("CategoryID add 1 eq 2", filter);
        }

        [Fact]
        public void SubEqualNumeric()
        {
            var x = ODataFilter.Context;
            string filter = x.CategoryID - 1 == 2;
            Assert.Equal("CategoryID sub 1 eq 2", filter);
        }

        [Fact]
        public void MulEqualNumeric()
        {
            var x = ODataFilter.Context;
            string filter = x.CategoryID * 1 == 2;
            Assert.Equal("CategoryID mul 1 eq 2", filter);
        }

        [Fact]
        public void DivEqualNumeric()
        {
            var x = ODataFilter.Context;
            string filter = x.CategoryID / 1 == 2;
            Assert.Equal("CategoryID div 1 eq 2", filter);
        }

        [Fact]
        public void ModEqualNumeric()
        {
            var x = ODataFilter.Context;
            string filter = x.CategoryID % 1 == 2;
            Assert.Equal("CategoryID mod 1 eq 2", filter);
        }

        [Fact]
        public void LengthOfStringEqual()
        {
            var x = ODataFilter.Context;
            string filter = x.ProductName.Length() == 4;
            Assert.Equal("length(ProductName) eq 4", filter);
        }

        [Fact]
        public void StringToLowerEqual()
        {
            var x = ODataFilter.Context;
            string filter = x.ProductName.ToLower() == "chai";
            Assert.Equal("tolower(ProductName) eq 'chai'", filter);
        }

        [Fact]
        public void StringToUpperEqual()
        {
            var x = ODataFilter.Context;
            string filter = x.ProductName.ToUpper() == "CHAI";
            Assert.Equal("toupper(ProductName) eq 'CHAI'", filter);
        }

        [Fact]
        public void StringStartsWithEqual()
        {
            var x = ODataFilter.Context;
            string filter = x.ProductName.StartsWith("Ch") == true;
            Assert.Equal("startswith(ProductName,'Ch') eq true", filter);
        }

        [Fact]
        public void StringEndsWithEqual()
        {
            var x = ODataFilter.Context;
            string filter = x.ProductName.EndsWith("Ch") == true;
            Assert.Equal("endswith(ProductName,'Ch') eq true", filter);
        }

        [Fact]
        public void StringContainsEqualTrue()
        {
            var x = ODataFilter.Context;
            string filter = x.ProductName.Contains("ai") == true;
            Assert.Equal("substringof('ai',ProductName) eq true", filter);
        }

        [Fact]
        public void StringContainsEqualFalse()
        {
            var x = ODataFilter.Context;
            string filter = x.ProductName.Contains("ai") == false;
            Assert.Equal("substringof('ai',ProductName) eq false", filter);
        }

        [Fact]
        public void IndexOfStringEqual()
        {
            var x = ODataFilter.Context;
            string filter = x.ProductName.IndexOf("ai") == 1;
            Assert.Equal("indexof(ProductName,'ai') eq 1", filter);
        }

        [Fact]
        public void SubstringWithPositionEqual()
        {
            var x = ODataFilter.Context;
            string filter = x.ProductName.Substring(1) == "hai";
            Assert.Equal("substring(ProductName,1) eq 'hai'", filter);
        }

        [Fact]
        public void SubstringWithPositionAndLengthEqual()
        {
            var x = ODataFilter.Context;
            string filter = x.ProductName.Substring(1,2) == "ha";
            Assert.Equal("substring(ProductName,1,2) eq 'ha'", filter);
        }

        [Fact]
        public void ReplaceStringEqual()
        {
            var x = ODataFilter.Context;
            string filter = x.ProductName.Replace("a","o") == "Choi";
            Assert.Equal("replace(ProductName,'a','o') eq 'Choi'", filter);
        }

        [Fact]
        public void TrimEqual()
        {
            var x = ODataFilter.Context;
            string filter = x.ProductName.Trim() == "Chai";
            Assert.Equal("trim(ProductName) eq 'Chai'", filter);
        }

        [Fact]
        public void ConcatEqual()
        {
            var x = ODataFilter.Context;
            string filter = x.ProductName.Concat("Chai") == "ChaiChai";
            Assert.Equal("concat(ProductName,'Chai') eq 'ChaiChai'", filter);
        }

        [Fact]
        public void DayEqual()
        {
            var x = ODataFilter.Context;
            string filter = x.CreationTime.Day == 1;
            Assert.Equal("day(CreationTime) eq 1", filter);
        }

        [Fact]
        public void MonthEqual()
        {
            var x = ODataFilter.Context;
            string filter = x.CreationTime.Month == 2;
            Assert.Equal("month(CreationTime) eq 2", filter);
        }

        [Fact]
        public void YearEqual()
        {
            var x = ODataFilter.Context;
            string filter = x.CreationTime.Year == 3;
            Assert.Equal("year(CreationTime) eq 3", filter);
        }

        [Fact]
        public void HourEqual()
        {
            var x = ODataFilter.Context;
            string filter = x.CreationTime.Hour == 4;
            Assert.Equal("hour(CreationTime) eq 4", filter);
        }

        [Fact]
        public void MinuteEqual()
        {
            var x = ODataFilter.Context;
            string filter = x.CreationTime.Minute == 5;
            Assert.Equal("minute(CreationTime) eq 5", filter);
        }

        [Fact]
        public void SecondEqual()
        {
            var x = ODataFilter.Context;
            string filter = x.CreationTime.Second == 6;
            Assert.Equal("second(CreationTime) eq 6", filter);
        }

        [Fact]
        public void RoundEqual()
        {
            var x = ODataFilter.Context;
            string filter = x.Price.Round() == 1;
            Assert.Equal("round(Price) eq 1", filter);
        }

        [Fact]
        public void FloorEqual()
        {
            var x = ODataFilter.Context;
            string filter = x.Price.Floor() == 1;
            Assert.Equal("floor(Price) eq 1", filter);
        }

        [Fact]
        public void CeilingEqual()
        {
            var x = ODataFilter.Context;
            string filter = x.Price.Ceiling() == 2;
            Assert.Equal("ceiling(Price) eq 2", filter);
        }
    }
}