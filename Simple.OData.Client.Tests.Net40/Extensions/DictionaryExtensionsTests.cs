using System;
using System.Collections.Generic;
using Xunit;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client.Tests
{
    public class DictionaryExtensionsTests
    {
        class ClassType
        {
            public string StringField;
            public string[] StringCollectionField;

            public string StringProperty { get; set; }
            public string StringPropertyPrivateSetter { get; private set; }
            public int IntProperty { get; set; }
            public string[] StringCollectionProperty { get; set; }
            public int[] IntCollectionProperty { get; set; }
            public SubclassType CompoundProperty { get; set; }
            public SubclassType[] CompoundCollectionProperty { get; set; }
        }

        class SubclassType
        {
            public string StringProperty { get; set; }
            public int IntProperty { get; set; }
        }

        [Fact]
        public void AsObjectPrimitiveProperties()
        {
            var dict = new Dictionary<string, object>()
            {
                { "StringProperty", "a" }, 
                { "IntProperty", 1 },
            };

            var value = dict.AsObjectOfType<ClassType>();
            Assert.Equal("a", value.StringProperty);
            Assert.Equal(1, value.IntProperty);
        }

        [Fact]
        public void AsObjectUnknownProperty()
        {
            var dict = new Dictionary<string, object>()
            {
                { "StringProperty", "a" }, 
                { "IntProperty", 1 },
                { "UnknownProperty", "u" }
            };

            var value = dict.AsObjectOfType<ClassType>();
            Assert.Equal("a", value.StringProperty);
            Assert.Equal(1, value.IntProperty);
        }

        [Fact]
        public void AsObjectPrivateSetter()
        {
            var dict = new Dictionary<string, object>()
            {
                { "StringProperty", "a" }, 
                { "IntProperty", 1 },
                { "StringPropertyPrivateSetter", "p" }
            };

            var value = dict.AsObjectOfType<ClassType>();
            Assert.Equal("a", value.StringProperty);
            Assert.Equal(1, value.IntProperty);
            Assert.Equal("p", value.StringPropertyPrivateSetter);
        }

        [Fact]
        public void AsObjectField()
        {
            var dict = new Dictionary<string, object>()
            {
                { "StringProperty", "a" }, 
                { "IntProperty", 1 },
                { "StringField", "f" }
            };

            var value = dict.AsObjectOfType<ClassType>();
            Assert.Equal("a", value.StringProperty);
            Assert.Equal(1, value.IntProperty);
            Assert.Equal(null, value.StringField);
        }

        [Fact]
        public void AsObjectStringCollection()
        {
            var dict = new Dictionary<string, object>()
            {
                { "StringProperty", "a" }, 
                { "IntProperty", 1 },
                { "StringCollectionProperty", new [] {"x", "y", "z"}  }
            };

            var value = dict.AsObjectOfType<ClassType>();
            Assert.Equal("a", value.StringProperty);
            Assert.Equal(1, value.IntProperty);
            for (var index = 0; index < 3; index++)
            {
                Assert.Equal((dict["StringCollectionProperty"] as IList<string>)[index], value.StringCollectionProperty[index]);
            }
        }

        [Fact]
        public void AsObjectIntCollection()
        {
            var dict = new Dictionary<string, object>()
            {
                { "StringProperty", "a" }, 
                { "IntProperty", 1 },
                { "IntCollectionProperty", new [] {1, 2, 3}  }
            };

            var value = dict.AsObjectOfType<ClassType>();
            Assert.Equal("a", value.StringProperty);
            Assert.Equal(1, value.IntProperty);
            for (var index = 0; index < 3; index++)
            {
                Assert.Equal((dict["IntCollectionProperty"] as IList<int>)[index], value.IntCollectionProperty[index]);
            }
        }

        [Fact]
        public void AsObjectCompoundProperty()
        {
            var dict = new Dictionary<string, object>()
            {
                { "StringProperty", "a" }, 
                { "IntProperty", 1 },
                { "CompoundProperty", new Dictionary<string, object>() { { "StringProperty", "z" }, { "IntProperty", 0 } }  }
            };

            var value = dict.AsObjectOfType<ClassType>();
            Assert.Equal("a", value.StringProperty);
            Assert.Equal(1, value.IntProperty);
            Assert.Equal("z", value.CompoundProperty.StringProperty);
            Assert.Equal(0, value.CompoundProperty.IntProperty);
        }

        [Fact]
        public void AsObjectCompoundCollectionProperty()
        {
            var dict = new Dictionary<string, object>()
            {
                { "StringProperty", "a" }, 
                { "IntProperty", 1 },
                { "CompoundCollectionProperty", new[]
                    {
                        new Dictionary<string, object>() { { "StringProperty", "x" }, { "IntProperty", 1 } }, 
                        new Dictionary<string, object>() { { "StringProperty", "y" }, { "IntProperty", 2 } },
                        new Dictionary<string, object>() { { "StringProperty", "z" }, { "IntProperty", 3 } },
                    } 
                }
            };

            var value = dict.AsObjectOfType<ClassType>();
            Assert.Equal("a", value.StringProperty);
            Assert.Equal(1, value.IntProperty);
            for (var index = 0; index < 3; index++)
            {
                var kv = (dict["CompoundCollectionProperty"] as IList<IDictionary<string, object>>)[index];
                Assert.Equal(kv["StringProperty"], value.CompoundCollectionProperty[index].StringProperty);
                Assert.Equal(kv["IntProperty"], value.CompoundCollectionProperty[index].IntProperty);
            }
        }
    }
}
