using System.Linq;
using Xunit;

namespace Simple.OData.Client.Tests.Extensions
{
    public class TypeCacheTests
    {
        private ITypeCache _typeCache => TypeCaches.TypeCache("test", null);

        [Fact]
        public void GetAllProperties_BaseType()
        {
            Assert.Single(_typeCache.GetAllProperties(typeof(Transport)));
        }

        [Fact]
        public void GetAllProperties_DerivedType()
        {
            Assert.Equal(2, _typeCache.GetAllProperties(typeof(Ship)).Count());
        }

        [Fact]
        public void GetDeclaredProperties_ExcludeExplicitInterface()
        {
            Assert.Equal(5, _typeCache.GetAllProperties(typeof(Address)).Count());
        }

        [Fact]
        public void GetDeclaredProperties_BaseType()
        {
            Assert.Single(_typeCache.GetDeclaredProperties(typeof(Transport)));
        }

        [Fact]
        public void GetDeclaredProperties_DerivedType()
        {
            Assert.Single(_typeCache.GetDeclaredProperties(typeof(Ship)));
        }

        [Fact]
        public void GetNamedProperty_BaseType()
        {
            Assert.NotNull(_typeCache.GetNamedProperty(typeof(Transport), "TransportID"));
        }

        [Fact]
        public void GetNamedProperty_DerivedType()
        {
            Assert.NotNull(_typeCache.GetNamedProperty(typeof(Ship), "TransportID"));
            Assert.NotNull(_typeCache.GetNamedProperty(typeof(Ship), "ShipName"));
        }

        [Fact]
        public void GetDeclaredProperty_BaseType()
        {
            Assert.NotNull(_typeCache.GetDeclaredProperty(typeof(Transport), "TransportID"));
        }

        [Fact]
        public void GetDeclaredProperty_DerivedType()
        {
            Assert.Null(_typeCache.GetDeclaredProperty(typeof(Ship), "TransportID"));
            Assert.NotNull(_typeCache.GetDeclaredProperty(typeof(Ship), "ShipName"));
        }
    }
}