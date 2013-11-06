using System;
using Xunit;

namespace Simple.OData.Client.Tests
{
    public class FilterExpressionTests
    {
        [Fact]
        public void And()
        {
            var x = ODataDynamic.Expression;
            var filter = x.CategoryID == 1 && x.ProductName == "Chai";
            Assert.Equal("CategoryID eq 1 and ProductName eq 'Chai'", filter.ToString());
        }

        [Fact]
        public void Or()
        {
            var x = ODataDynamic.Expression;
            var filter = x.ProductName == "Chai" || x.CategoryID == 1;
            Assert.Equal("ProductName eq 'Chai' or CategoryID eq 1", filter.ToString());
        }

        [Fact]
        public void Not()
        {
            var x = ODataDynamic.Expression;
            var filter = !(x.ProductName == "Chai");
            Assert.Equal("not(ProductName eq 'Chai')", filter.ToString());
        }

        [Fact]
        public void Precedence()
        {
            var x = ODataDynamic.Expression;
            var filter = (x.CategoryID == 1 || x.CategoryID == 2) && x.ProductName == "Chai";
            Assert.Equal("(CategoryID eq 1 or CategoryID eq 2) and ProductName eq 'Chai'", filter.ToString());
        }

        [Fact]
        public void EqualString()
        {
            var x = ODataDynamic.Expression;
            var filter = x.ProductName == "Chai";
            Assert.Equal("ProductName eq 'Chai'", filter.ToString());
        }

        [Fact]
        public void EqualNumeric()
        {
            var x = ODataDynamic.Expression;
            var filter = x.CategoryID == 1;
            Assert.Equal("CategoryID eq 1", filter.ToString());
        }

        [Fact]
        public void NotEqualNumeric()
        {
            var x = ODataDynamic.Expression;
            var filter = x.CategoryID != 1;
            Assert.Equal("CategoryID ne 1", filter.ToString());
        }

        [Fact]
        public void GreaterNumeric()
        {
            var x = ODataDynamic.Expression;
            var filter = x.CategoryID > 1;
            Assert.Equal("CategoryID gt 1", filter.ToString());
        }

        [Fact]
        public void GreaterOrEqualNumeric()
        {
            var x = ODataDynamic.Expression;
            var filter = x.CategoryID >= 1.5;
            Assert.Equal("CategoryID ge 1.5", filter.ToString());
        }

        [Fact]
        public void LessNumeric()
        {
            var x = ODataDynamic.Expression;
            var filter = x.CategoryID < 1;
            Assert.Equal("CategoryID lt 1", filter.ToString());
        }

        [Fact]
        public void LessOrEqualNumeric()
        {
            var x = ODataDynamic.Expression;
            var filter = x.CategoryID <= 1;
            Assert.Equal("CategoryID le 1", filter.ToString());
        }

        [Fact]
        public void AddEqualNumeric()
        {
            var x = ODataDynamic.Expression;
            var filter = x.CategoryID + 1 == 2;
            Assert.Equal("CategoryID add 1 eq 2", filter.ToString());
        }

        [Fact]
        public void SubEqualNumeric()
        {
            var x = ODataDynamic.Expression;
            var filter = x.CategoryID - 1 == 2;
            Assert.Equal("CategoryID sub 1 eq 2", filter.ToString());
        }

        [Fact]
        public void MulEqualNumeric()
        {
            var x = ODataDynamic.Expression;
            var filter = x.CategoryID * 1 == 2;
            Assert.Equal("CategoryID mul 1 eq 2", filter.ToString());
        }

        [Fact]
        public void DivEqualNumeric()
        {
            var x = ODataDynamic.Expression;
            var filter = x.CategoryID / 1 == 2;
            Assert.Equal("CategoryID div 1 eq 2", filter.ToString());
        }

        [Fact]
        public void ModEqualNumeric()
        {
            var x = ODataDynamic.Expression;
            var filter = x.CategoryID % 1 == 2;
            Assert.Equal("CategoryID mod 1 eq 2", filter.ToString());
        }

        [Fact]
        public void EqualLong()
        {
            var x = ODataDynamic.Expression;
            var filter = x.CategoryID == 1L;
            Assert.Equal("CategoryID eq 1L", filter.ToString());
        }

        [Fact]
        public void EqualDecimal()
        {
            var x = ODataDynamic.Expression;
            var filter = x.Total == 1M;
            Assert.Equal("Total eq 1.00M", filter.ToString());
        }

        [Fact]
        public void EqualDecimalWithFractionalPart()
        {
            var x = ODataDynamic.Expression;
            var filter = x.Total == 1.23M;
            Assert.Equal("Total eq 1.23M", filter.ToString());
        }

        [Fact]
        public void EqualGuid()
        {
            var x = ODataDynamic.Expression;
            var filter = x.CategoryID == Guid.Empty;
            Assert.Equal("CategoryID eq guid'00000000-0000-0000-0000-000000000000'", filter.ToString());
        }

        [Fact]
        public void EqualDateTime()
        {
            var x = ODataDynamic.Expression;
            var filter = x.Updated == new DateTime(2013, 1, 1);
            Assert.Equal("Updated eq datetime'2013-01-01T00:00:00.0000000Z'", filter.ToString());
        }

        [Fact]
        public void EqualDateTimeOffset()
        {
            var x = ODataDynamic.Expression;
            var filter = x.Updated == new DateTimeOffset(new DateTime(2013, 1, 1));
            Assert.Equal("Updated eq datetimeoffset'2013-01-01T00:00:00.0000000Z'", filter.ToString());
        }

        [Fact]
        public void EqualTimeSpan()
        {
            var x = ODataDynamic.Expression;
            var filter = x.Updated == new TimeSpan(1, 2, 3);
            Assert.Equal("Updated eq time'01:02:03'", filter.ToString());
        }

        [Fact]
        public void LengthOfStringEqual()
        {
            var x = ODataDynamic.Expression;
            var filter = x.ProductName.Length() == 4;
            Assert.Equal("length(ProductName) eq 4", filter.ToString());
        }

        [Fact]
        public void StringToLowerEqual()
        {
            var x = ODataDynamic.Expression;
            var filter = x.ProductName.ToLower() == "chai";
            Assert.Equal("tolower(ProductName) eq 'chai'", filter.ToString());
        }

        [Fact]
        public void StringToUpperEqual()
        {
            var x = ODataDynamic.Expression;
            var filter = x.ProductName.ToUpper() == "CHAI";
            Assert.Equal("toupper(ProductName) eq 'CHAI'", filter.ToString());
        }

        [Fact]
        public void StringStartsWithEqual()
        {
            var x = ODataDynamic.Expression;
            var filter = x.ProductName.StartsWith("Ch") == true;
            Assert.Equal("startswith(ProductName,'Ch') eq true", filter.ToString());
        }

        [Fact]
        public void StringEndsWithEqual()
        {
            var x = ODataDynamic.Expression;
            var filter = x.ProductName.EndsWith("Ch") == true;
            Assert.Equal("endswith(ProductName,'Ch') eq true", filter.ToString());
        }

        [Fact]
        public void StringContainsEqualTrue()
        {
            var x = ODataDynamic.Expression;
            var filter = x.ProductName.Contains("ai") == true;
            Assert.Equal("substringof('ai',ProductName) eq true", filter.ToString());
        }

        [Fact]
        public void StringContainsEqualFalse()
        {
            var x = ODataDynamic.Expression;
            var filter = x.ProductName.Contains("ai") == false;
            Assert.Equal("substringof('ai',ProductName) eq false", filter.ToString());
        }

        [Fact]
        public void StringContains()
        {
            var x = ODataDynamic.Expression;
            var filter = x.ProductName.Contains("ai");
            Assert.Equal("substringof('ai',ProductName)", filter.ToString());
        }

        [Fact]
        public void StringNotContains()
        {
            var x = ODataDynamic.Expression;
            var filter = !x.ProductName.Contains("ai");
            Assert.Equal("not substringof('ai',ProductName)", filter.ToString());
        }

        [Fact]
        public void IndexOfStringEqual()
        {
            var x = ODataDynamic.Expression;
            var filter = x.ProductName.IndexOf("ai") == 1;
            Assert.Equal("indexof(ProductName,'ai') eq 1", filter.ToString());
        }

        [Fact]
        public void SubstringWithPositionEqual()
        {
            var x = ODataDynamic.Expression;
            var filter = x.ProductName.Substring(1) == "hai";
            Assert.Equal("substring(ProductName,1) eq 'hai'", filter.ToString());
        }

        [Fact]
        public void SubstringWithPositionAndLengthEqual()
        {
            var x = ODataDynamic.Expression;
            var filter = x.ProductName.Substring(1,2) == "ha";
            Assert.Equal("substring(ProductName,1,2) eq 'ha'", filter.ToString());
        }

        [Fact]
        public void ReplaceStringEqual()
        {
            var x = ODataDynamic.Expression;
            var filter = x.ProductName.Replace("a","o") == "Choi";
            Assert.Equal("replace(ProductName,'a','o') eq 'Choi'", filter.ToString());
        }

        [Fact]
        public void TrimEqual()
        {
            var x = ODataDynamic.Expression;
            var filter = x.ProductName.Trim() == "Chai";
            Assert.Equal("trim(ProductName) eq 'Chai'", filter.ToString());
        }

        [Fact]
        public void ConcatEqual()
        {
            var x = ODataDynamic.Expression;
            var filter = x.ProductName.Concat("Chai") == "ChaiChai";
            Assert.Equal("concat(ProductName,'Chai') eq 'ChaiChai'", filter.ToString());
        }

        [Fact]
        public void DayEqual()
        {
            var x = ODataDynamic.Expression;
            var filter = x.CreationTime.Day == 1;
            Assert.Equal("day(CreationTime) eq 1", filter.ToString());
        }

        [Fact]
        public void MonthEqual()
        {
            var x = ODataDynamic.Expression;
            var filter = x.CreationTime.Month == 2;
            Assert.Equal("month(CreationTime) eq 2", filter.ToString());
        }

        [Fact]
        public void YearEqual()
        {
            var x = ODataDynamic.Expression;
            var filter = x.CreationTime.Year == 3;
            Assert.Equal("year(CreationTime) eq 3", filter.ToString());
        }

        [Fact]
        public void HourEqual()
        {
            var x = ODataDynamic.Expression;
            var filter = x.CreationTime.Hour == 4;
            Assert.Equal("hour(CreationTime) eq 4", filter.ToString());
        }

        [Fact]
        public void MinuteEqual()
        {
            var x = ODataDynamic.Expression;
            var filter = x.CreationTime.Minute == 5;
            Assert.Equal("minute(CreationTime) eq 5", filter.ToString());
        }

        [Fact]
        public void SecondEqual()
        {
            var x = ODataDynamic.Expression;
            var filter = x.CreationTime.Second == 6;
            Assert.Equal("second(CreationTime) eq 6", filter.ToString());
        }

        [Fact]
        public void RoundEqual()
        {
            var x = ODataDynamic.Expression;
            var filter = x.Price.Round() == 1;
            Assert.Equal("round(Price) eq 1", filter.ToString());
        }

        [Fact]
        public void FloorEqual()
        {
            var x = ODataDynamic.Expression;
            var filter = x.Price.Floor() == 1;
            Assert.Equal("floor(Price) eq 1", filter.ToString());
        }

        [Fact]
        public void CeilingEqual()
        {
            var x = ODataDynamic.Expression;
            var filter = x.Price.Ceiling() == 2;
            Assert.Equal("ceiling(Price) eq 2", filter.ToString());
        }
    }
}