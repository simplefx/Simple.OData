using System.Threading.Tasks;
using Xunit;

namespace Simple.OData.Client.Tests
{
    public class FindNuGetTests
    {
        [Fact]
        public async Task FindEntryNuGetV1()
        {
            var client = new ODataClient("http://nuget.org/api/v1");
            var package = await client.FindEntryAsync("Packages?$filter=Title eq 'EntityFramework'");
            Assert.NotNull(package["Id"]);
            Assert.NotNull(package["Authors"]);
        }

        [Fact]
        public async Task FindEntryNuGetV2()
        {
            var client = new ODataClient("http://nuget.org/api/v2");
            var package = await client.FindEntryAsync("Packages?$filter=Title eq 'EntityFramework'");
            Assert.NotNull(package["Id"]);
        }

        [Fact]
        public async Task FindEntryNuGetV2_FieldWithAnnotation()
        {
            var client = new ODataClient("http://nuget.org/api/v2");
            var package = await client.FindEntryAsync("Packages?$filter=Title eq 'EntityFramework'");
            Assert.NotNull(package["Authors"]);
        }
    }
}