using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using Entry = System.Collections.Generic.Dictionary<string, object>;

namespace Simple.OData.Client.Tests
{
#if ODATA_V3
    public class ErrorODataTestsV2Atom : ErrorODataTests
    {
        public ErrorODataTestsV2Atom() : base(ODataV2ReadWriteUri, ODataPayloadFormat.Atom, 2) { }
    }

    public class ErrorODataTestsV2Json : ErrorODataTests
    {
        public ErrorODataTestsV2Json() : base(ODataV2ReadWriteUri, ODataPayloadFormat.Json, 2) { }
    }

    public class ErrorODataTestsV3Atom : ErrorODataTests
    {
        public ErrorODataTestsV3Atom() : base(ODataV3ReadWriteUri, ODataPayloadFormat.Atom, 3) { }
    }

    public class ErrorODataTestsV3Json : ErrorODataTests
    {
        public ErrorODataTestsV3Json() : base(ODataV3ReadWriteUri, ODataPayloadFormat.Json, 3) { }
    }
#endif

#if ODATA_V4
    public class ErrorODataTestsV4Json : ErrorODataTests
    {
        public ErrorODataTestsV4Json() : base(ODataV4ReadWriteUri, ODataPayloadFormat.Json, 4) { }
    }
#endif

    public abstract class ErrorODataTests : ODataTestBase
    {
        protected ErrorODataTests(string serviceUri, ODataPayloadFormat payloadFormat, int version)
            : base(serviceUri, payloadFormat, version)
        {
        }

        [Fact]
        public async Task ErrorContent()
        {
            try
            {
                await _client
                    .For("Products")
                    .Filter("NonExistingProperty eq 1")
                    .FindEntryAsync();

                Assert.False(true, "Expected exception");
            }
            catch (WebRequestException ex)
            {
                Assert.NotNull(ex.Response);
            }
            catch (Exception)
            {
                Assert.False(true, "Expected WebRequestException");
            }
        }
    }
}
