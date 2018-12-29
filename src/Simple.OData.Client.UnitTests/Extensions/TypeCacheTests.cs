using System.Linq;

using Xunit;

namespace Simple.OData.Client.Tests.Extensions
{
    public class TypeCacheTests
    {
        [Fact]
        public void GetAllProperties_BaseType()
        {
            var typeCache = TypeCaches.Global;

            Assert.Single(typeCache.GetAllProperties(typeof(Transport)));
        }

        [Fact]
        public void GetAllProperties_DerivedType()
        {
            var typeCache = TypeCaches.Global;

            Assert.Equal(2, typeCache.GetAllProperties(typeof(Ship)).Count());
        }

        [Fact]
        public void GetDeclaredProperties_ExcludeExplicitInterface()
        {
            var typeCache = TypeCaches.Global;

            Assert.Equal(5, typeCache.GetAllProperties(typeof(Address)).Count());
        }

        [Fact]
        public void GetDeclaredProperties_BaseType()
        {
            var typeCache = TypeCaches.Global;

            Assert.Single(typeCache.GetDeclaredProperties(typeof(Transport)));
        }

        [Fact]
        public void GetDeclaredProperties_DerivedType()
        {
            var typeCache = TypeCaches.Global;

            Assert.Single(typeCache.GetDeclaredProperties(typeof(Ship)));
        }

        [Fact]
        public void GetNamedProperty_BaseType()
        {
            var typeCache = TypeCaches.Global;

            Assert.NotNull(typeCache.GetNamedProperty(typeof(Transport), "TransportID"));
        }

        [Fact]
        public void GetNamedProperty_DerivedType()
        {
            var typeCache = TypeCaches.Global;

            Assert.NotNull(typeCache.GetNamedProperty(typeof(Ship), "TransportID"));
            Assert.NotNull(typeCache.GetNamedProperty(typeof(Ship), "ShipName"));
        }

        [Fact]
        public void GetDeclaredProperty_BaseType()
        {
            var typeCache = TypeCaches.Global;

            Assert.NotNull(typeCache.GetDeclaredProperty(typeof(Transport), "TransportID"));
        }

        [Fact]
        public void GetDeclaredProperty_DerivedType()
        {
            var typeCache = TypeCaches.Global;

            Assert.Null(typeCache.GetDeclaredProperty(typeof(Ship), "TransportID"));
            Assert.NotNull(typeCache.GetDeclaredProperty(typeof(Ship), "ShipName"));
        }
    }
}