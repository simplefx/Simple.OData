using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NExtLib.TestExtensions;

namespace NExtLib.Unit
{
    using Xunit;

    public static class ShouldExtensions
    {
        public static void ShouldEqual<T>(this T actual, T expected)
        {
            Assert.Equal(expected, actual);
        }

        public static void ShouldEqual<T>(this T actual, T expected, string message)
        {
            Assert.Equal(expected, actual);
        }

        public static void ShouldBeNull<T>(this T actual)
            where T : class
        {
            Assert.Null(actual);
        }

        public static void ShouldNotBeNull<T>(this T actual)
        where T : class
        {
            Assert.NotNull(actual);
        }

        public static void Should<T>(this T actual, IBinaryTest equal, T expected)
        {
            if (equal == null) throw new ArgumentNullException("equal");

            equal.Run(expected, actual);
        }

        public static void Should<T>(this IEnumerable<T> actual, IEnumerableTest test, T expected)
        {
            if (test == null) throw new ArgumentNullException("test");

            test.RunTest(expected, actual);
        }
    }
}
