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

        protected override async Task DeleteTestData()
        {
            try
            {
            }
            catch (Exception)
            {
            }
        }
    }
}