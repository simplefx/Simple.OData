using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using Simple.OData.Client.Extensions;
using Simple.OData.Client.TestUtils;
using Xunit;
#if NETFX_CORE
using Windows.Storage;
#endif

namespace Simple.OData.Client.Tests
{
    
    public class XmlElementAsDictionaryReadTests
    {
        [Fact]
        public async Task FirstDescendantIsTweetOne()
        {
            XmlElementAsDictionary actual = XmlElementAsDictionary.ParseDescendants(Properties.XmlSamples.TwitterStatusesSample, "status").First();
            actual["text"].Value.ShouldEqual("Tweet one.");
        }

        [Fact]
        public async Task SecondDescendantIsTweetTwo()
        {
            XmlElementAsDictionary actual = XmlElementAsDictionary.ParseDescendants(Properties.XmlSamples.TwitterStatusesSample, "status").Skip(1).First();
            actual["text"].Value.ShouldEqual("Tweet two.");
        }

        [Fact]
        public async Task ParseDescendantsReturnsTwoItems()
        {
            XmlElementAsDictionary.ParseDescendants(Properties.XmlSamples.TwitterStatusesSample, "status").Count().ShouldEqual(2);
        }

        [Fact]
        public async Task UserNameReturnedCorrectly()
        {
            var one = XmlElementAsDictionary.ParseDescendants(Properties.XmlSamples.TwitterStatusesSample, "status").First();
            one["user"]["name"].Value.ShouldEqual("Doug Williams");
        }
    }
}
