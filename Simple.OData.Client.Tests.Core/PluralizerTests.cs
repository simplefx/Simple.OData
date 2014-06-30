using Simple.OData.Client.Extensions;
using Xunit;
using Xunit.Extensions;

namespace Simple.OData.Client.Tests
{
    public class PluralizerTests
    {
        private readonly SimplePluralizer _pluralizer = new SimplePluralizer();

        [Theory]
        [InlineData("Person", "Persons")]
        [InlineData("Day", "Days")]
        [InlineData("Dummy", "Dummies")]
        [InlineData("Access", "Accesses")]
        public void PluralizeWord(string word, string expectedResult)
        {
            Assert.Equal(expectedResult, _pluralizer.Pluralize(word));
        }

        [Theory]
        [InlineData("Persons", "Person")]
        [InlineData("Days", "Day")]
        [InlineData("Dummies", "Dummy")]
        [InlineData("Accesses", "Access")]
        public void SingularizeWord(string word, string expectedResult)
        {
            Assert.Equal(expectedResult, _pluralizer.Singularize(word));
        }
    }
}