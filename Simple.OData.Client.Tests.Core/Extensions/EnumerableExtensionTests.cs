using System.Collections.Generic;
using Simple.OData.Client.Extensions;
using Simple.OData.Client.TestUtils;
using Xunit;

namespace Simple.OData.Client.Tests
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
