using System.Collections.Generic;
using System.Linq;
using NExtLib.Unit;
using Xunit;
using Simple.NExtLib.Tests.Properties;
using Simple.NExtLib.Xml;

namespace Simple.NExtLib.Tests.Xml
{
    
    public class XmlElementAsDictionaryReadTests
    {
        private static IEnumerable<XmlElementAsDictionary> ParseDescendantsUnderTest
        {
            get { return XmlElementAsDictionary.ParseDescendants(Resources.TwitterStatusesSample, "status"); }
        }

        [Fact]
        public void FirstDescendantIsTweetOne()
        {
            XmlElementAsDictionary actual = ParseDescendantsUnderTest.First();
            actual["text"].Value.ShouldEqual("Tweet one.");
        }

        [Fact]
        public void SecondDescendantIsTweetTwo()
        {
            XmlElementAsDictionary actual = ParseDescendantsUnderTest.Skip(1).First();
            actual["text"].Value.ShouldEqual("Tweet two.");
        }

        [Fact]
        public void ParseDescendantsReturnsTwoItems()
        {
            ParseDescendantsUnderTest.Count().ShouldEqual(2);
        }

        [Fact]
        public void UserNameReturnedCorrectly()
        {
            var one = ParseDescendantsUnderTest.First();

            one["user"]["name"].Value.ShouldEqual("Doug Williams");
        }
    }
}