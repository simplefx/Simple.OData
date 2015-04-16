using System;
using System.Threading.Tasks;

namespace Simple.OData.Client.Tests
{
    public class TripPinTestBase : TestBase
    {
        protected TripPinTestBase(string serviceUri, ODataPayloadFormat payloadFormat)
            : base(serviceUri, payloadFormat)
        {
        }

#pragma warning disable 1998
        protected override async Task DeleteTestData()
        {
            try
            {
            }
            catch (Exception)
            {
            }
        }
#pragma warning restore 1998
    }
}
