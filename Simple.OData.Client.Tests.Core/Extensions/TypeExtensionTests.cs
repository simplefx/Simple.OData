using System.Linq;
using Simple.OData.Client.Extensions;
using Xunit;

namespace Simple.OData.Client.Tests
{
    public class TypeExtensionTests
    {
        [Fact]
        public void GetAllProperties_BaseType()
        {
            Assert.Equal(1, typeof(Transport).GetAllProperties().Count());
        }

        [Fact]
        public void GetAllProperties_DerivedType()
        {
            Assert.Equal(2, typeof(Ship).GetAllProperties().Count());
        }

        [Fact]
        public void GetDeclaredProperties_BaseType()
        {
            Assert.Equal(1, typeof(Transport).GetDeclaredProperties().Count());
        }

        [Fact]
        public void GetDeclaredProperties_DerivedType()
        {
            Assert.Equal(1, typeof(Ship).GetDeclaredProperties().Count());
        }

        [Fact]
        public void GetAnyProperty_BaseType()
        {
            Assert.NotNull(typeof(Transport).GetAnyProperty("TransportID"));
        }

        [Fact]
        public void GetAnyProperty_DerivedType()
        {
            Assert.NotNull(typeof(Ship).GetAnyProperty("TransportID"));
            Assert.NotNull(typeof(Ship).GetAnyProperty("ShipName"));
        }

        [Fact]
        public void GetDeclaredProperty_BaseType()
        {
            Assert.NotNull(typeof(Transport).GetDeclaredProperty("TransportID"));
        }

        [Fact]
        public void GetDeclaredProperty_DerivedType()
        {
            Assert.Null(typeof(Ship).GetDeclaredProperty("TransportID"));
            Assert.NotNull(typeof(Ship).GetDeclaredProperty("ShipName"));
        }
    }
}