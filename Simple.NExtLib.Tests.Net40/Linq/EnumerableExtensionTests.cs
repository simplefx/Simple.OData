using System.Collections.Generic;
using NExtLib.TestUtils;
using Xunit;
using Simple.NExtLib.Linq;

namespace Simple.NExtLib.Tests.Linq
{
    public class EnumerableExtensionTests
    {
        private readonly IEnumerable<string> TestList = new List<string> { "Foo", "Bar", "Quux" };

        [Fact]
        public void TestWithIndex()
        {
            int expectedIndex = 0;

            foreach (var item in TestList.WithIndex())
            {
                item.Item2.ShouldEqual(expectedIndex);
                expectedIndex++;
            }
        }
    }
}
