using System;
using System.CodeDom;
using Xunit;
using Xunit.Extensions;
using Xunit.Sdk;

namespace Simple.OData.Client.Tests
{
    public class ValueConversionTests
    {
        [Theory]
        [InlineData(1, typeof(int), typeof(byte))]
        [InlineData(1, typeof(int), typeof(short))]
        [InlineData(1, typeof(int), typeof(long))]
        [InlineData(1, typeof(int), typeof(decimal))]
        [InlineData(1, typeof(int), typeof(float))]
        [InlineData(1, typeof(int), typeof(double))]
        [InlineData(1, typeof(double), typeof(byte))]
        [InlineData(1, typeof(double), typeof(short))]
        [InlineData(1, typeof(double), typeof(long))]
        [InlineData(1, typeof(double), typeof(decimal))]
        [InlineData(1, typeof(double), typeof(float))]
        [InlineData("2014-02-01T12:00:00.123", typeof(DateTimeOffset), typeof(DateTime))]
        public void TryConvert(object value, Type type1, Type type2)
        {
            var sourceValue = ChangeType(value, type1);
            object targetValue;
            var result = Utils.TryConvert(sourceValue, type2, out targetValue);
            Assert.Equal(true, result);
            Assert.Equal(ChangeType(sourceValue, type2), ChangeType(targetValue, type2));

            sourceValue = ChangeType(value, type2);
            result = Utils.TryConvert(sourceValue, type1, out targetValue);
            Assert.Equal(true, result);
            Assert.Equal(ChangeType(sourceValue, type1), ChangeType(targetValue, type1));
        }

        private object ChangeType(object value, Type type)
        {
            if (type == typeof (DateTime))
                return DateTime.Parse(value.ToString());
            if (type == typeof(DateTimeOffset))
                return DateTimeOffset.Parse(value.ToString());

            return Convert.ChangeType(value, type);
        }
    }
}