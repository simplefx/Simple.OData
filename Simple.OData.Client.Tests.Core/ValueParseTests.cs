using System;
using Xunit;

namespace Simple.OData.Client.Tests
{
    public class ValueParseTests
    {
        private ValueParser _parser;

        public ValueParseTests()
        {
            _parser = new ValueParser(null);
        }

        [Fact]
        public void ParseBinaryNoPrefix()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.Binary };
            var value = _parser.ParseValue("123456789ABCDEF", propertyType);
            Assert.Equal(new Byte[] {0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF}, value);
        }

        [Fact]
        public void ParseBinaryWithLiteral()
        {
            var propertyType = new EdmPrimitivePropertyType() {Type = EdmType.Binary};
            var value = _parser.ParseValue("binary'23ABFF'", propertyType);
            Assert.Equal(new Byte[] { 0x23, 0xAB, 0xFF }, value);
        }

        [Fact]
        public void ParseBinaryWithPrefix()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.Binary };
            var value = _parser.ParseValue("X'23AB'", propertyType);
            Assert.Equal(new Byte[] { 0x23, 0xAB }, value);
        }

        [Fact]
        public void ParseBooleanValid()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.Boolean };
            var value = _parser.ParseValue("true", propertyType);
            Assert.Equal(true, value);
        }

        [Fact]
        public void ParseBooleanInvalid()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.Boolean };
            Assert.Throws<FormatException>(() => _parser.ParseValue("yes", propertyType));
        }

        [Fact]
        public void ParseByteValid()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.Byte };
            var value = _parser.ParseValue("123", propertyType);
            Assert.Equal((byte)123, value);
        }

        [Fact]
        public void ParseByteInvalid()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.Byte };
            Assert.Throws<OverflowException>(() => _parser.ParseValue("256", propertyType));
        }

        [Fact]
        public void ParseDateTimeNoPrefix()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.DateTime };
            var value = _parser.ParseValue("2000-12-12T12:00", propertyType);
            Assert.Equal(new DateTime(2000, 12, 12, 12, 0, 0), value);
        }

        [Fact]
        public void ParseDateTimeWithLiteral()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.DateTime };
            var value = _parser.ParseValue("datetime'2000-12-12T12:00'", propertyType);
            Assert.Equal(new DateTime(2000, 12, 12, 12, 0, 0), value);
        }

        [Fact]
        public void ParseDateTimeInvalid()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.DateTime };
            Assert.Throws<FormatException>(() => _parser.ParseValue("today", propertyType));
        }

        [Fact]
        public void ParseDecimalValid()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.Decimal };
            var value = _parser.ParseValue("-2.345M", propertyType);
            Assert.Equal(-2.345m, value);
        }

        [Fact]
        public void ParseDecimalInvalid()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.Decimal };
            Assert.Throws<FormatException>(() => _parser.ParseValue("12FF", propertyType));
        }

        [Fact]
        public void ParseDoubleValid()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.Double };
            var value = _parser.ParseValue("-2.345d", propertyType);
            Assert.Equal(-2.345d, value);
        }

        [Fact]
        public void ParseDoubleInvalid()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.Double };
            Assert.Throws<FormatException>(() => _parser.ParseValue("2.345M", propertyType));
        }

        [Fact]
        public void ParseSingleValid()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.Single };
            var value = _parser.ParseValue("-2.345f", propertyType);
            Assert.Equal(-2.345f, value);
        }

        [Fact]
        public void ParseSingleInvalid()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.Single };
            Assert.Throws<FormatException>(() => _parser.ParseValue("2.345d", propertyType));
        }

        [Fact]
        public void ParseGuidNoPrefix()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.Guid };
            var value = _parser.ParseValue("D675C324-7DD3-468E-9CF3-85BF3AD8B252", propertyType);
            Assert.Equal(new Guid("D675C324-7DD3-468E-9CF3-85BF3AD8B252"), value);
        }

        [Fact]
        public void ParseGuidWithLiteral()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.Guid };
            var value = _parser.ParseValue("guid'D675C324-7DD3-468E-9CF3-85BF3AD8B252'", propertyType);
            Assert.Equal(new Guid("D675C324-7DD3-468E-9CF3-85BF3AD8B252"), value);
        }

        [Fact]
        public void ParseGuidInvalid()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.Guid };
            Assert.Throws<FormatException>(() => _parser.ParseValue("guid'D675C3247DD3468E9CF385BF3AD8B2521234'", propertyType));
        }

        [Fact]
        public void ParseInt16Valid()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.Int16 };
            var value = _parser.ParseValue("123", propertyType);
            Assert.Equal((short)123, value);
        }

        [Fact]
        public void ParseInt16Invalid()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.Int16 };
            Assert.Throws<OverflowException>(() => _parser.ParseValue("123456", propertyType));
        }

        [Fact]
        public void ParseInt32Valid()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.Int32 };
            var value = _parser.ParseValue("123", propertyType);
            Assert.Equal(123, value);
        }

        [Fact]
        public void ParseInt32Invalid()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.Int32 };
            Assert.Throws<OverflowException>(() => _parser.ParseValue("123456789012", propertyType));
        }

        [Fact]
        public void ParseInt64Valid()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.Int64 };
            var value = _parser.ParseValue("123", propertyType);
            Assert.Equal(123l, value);
        }

        [Fact]
        public void ParseInt64Invalid()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.Int64 };
            Assert.Throws<FormatException>(() => _parser.ParseValue("123.45", propertyType));
        }

        [Fact]
        public void ParseSByteValid()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.SByte };
            var value = _parser.ParseValue("-123", propertyType);
            Assert.Equal((sbyte)-123, value);
        }

        [Fact]
        public void ParseSByteInvalid()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.SByte };
            Assert.Throws<OverflowException>(() => _parser.ParseValue("-200", propertyType));
        }

        [Fact]
        public void ParseTimeNoPrefix()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.Time };
            var value = _parser.ParseValue("12:00:11", propertyType);
            Assert.Equal(new TimeSpan(12, 0, 11), value);
        }

        [Fact]
        public void ParseTimeWithLiteral()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.Time };
            var value = _parser.ParseValue("time'12:00:11'", propertyType);
            Assert.Equal(new TimeSpan(12, 0, 11), value);
        }

        [Fact]
        public void ParseTimeInvalid()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.Time };
            Assert.Throws<FormatException>(() => _parser.ParseValue("2000-12-12T12:00", propertyType));
        }

        [Fact]
        public void ParseDateTimeOffsetNoPrefix()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.DateTimeOffset };
            var value = _parser.ParseValue("2000-12-12T12:00:11Z", propertyType);
            Assert.Equal(new DateTimeOffset(2000, 12, 12, 12, 0, 11, new TimeSpan()), value);
        }

        [Fact]
        public void ParseDateTimeOffsetWithLiteral()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.DateTimeOffset };
            var value = _parser.ParseValue("datetimeoffset'2000-12-12T12:00Z'", propertyType);
            Assert.Equal(new DateTimeOffset(2000, 12, 12, 12, 0, 0, new TimeSpan()), value);
        }

        [Fact]
        public void ParseDateTimeOffsetInvalid()
        {
            var propertyType = new EdmPrimitivePropertyType() { Type = EdmType.DateTimeOffset };
            Assert.Throws<FormatException>(() => _parser.ParseValue("today", propertyType));
        }
    }
}