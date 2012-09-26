using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NExtLib.TestExtensions
{
    using Xunit;

    public class ContainTest : IEnumerableTest
    {
        public void RunTest<T>(T expected, IEnumerable<T> actual)
        {
            Assert.True(actual.Contains(expected));
        }
    }
}
