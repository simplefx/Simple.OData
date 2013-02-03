using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using Xunit;
#if NETFX_CORE
using Windows.Storage;
#endif
using NExtLib.TestUtils;
using Simple.NExtLib.Xml;

namespace Simple.NExtLib.Tests.Xml
{
    
    public class XmlElementAsDictionaryReadTests
    {
        [Fact]
        public void FirstDescendantIsTweetOne()
        {
            XmlElementAsDictionary actual = XmlElementAsDictionary.ParseDescendants(Properties.Resources.TwitterStatusesSample, "status").First();
            actual["text"].Value.ShouldEqual("Tweet one.");
        }

        [Fact]
        public void SecondDescendantIsTweetTwo()
        {
            XmlElementAsDictionary actual = XmlElementAsDictionary.ParseDescendants(Properties.Resources.TwitterStatusesSample, "status").Skip(1).First();
            actual["text"].Value.ShouldEqual("Tweet two.");
        }

        [Fact]
        public void ParseDescendantsReturnsTwoItems()
        {
            XmlElementAsDictionary.ParseDescendants(Properties.Resources.TwitterStatusesSample, "status").Count().ShouldEqual(2);
        }

        [Fact]
        public void UserNameReturnedCorrectly()
        {
            var one = XmlElementAsDictionary.ParseDescendants(Properties.Resources.TwitterStatusesSample, "status").First();
            one["user"]["name"].Value.ShouldEqual("Doug Williams");
        }
    }
}
