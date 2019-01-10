using Xunit;

namespace Simple.OData.Client.Tests.Core
{
    public class TypeConverterCacheTests
    {
        [Fact]
        public void CachePerUri()
        {
            var c1 = CustomConverters.Converter("foo");
            var c2 = CustomConverters.Converter("bar");

            Assert.NotSame(c1, c2);
        }

        [Fact]
        public void SameCacheForUri()
        {
            var c1 = CustomConverters.Converter("foo");
            var c2 = CustomConverters.Converter("foo");

            Assert.Same(c1, c2);
        }

        [Fact]
        public void GlobalConverters()
        {
            var c1 = CustomConverters.Converter("global");
            var c2 = CustomConverters.Global;

            Assert.Same(c1, c2);
        }
    }
}