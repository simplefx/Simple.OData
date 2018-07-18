using System;
using System.Threading;
using BenchmarkDotNet.Running;
using Simple.OData.Client.Tests;

namespace Simple.OData.Client.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            //BenchmarkRunner.Run<CrmEmployee>();
            BenchmarkRunner.Run<TripPinPeople>();
        }
    }

    public static class Utils
    {
        public static ODataClient GetClient(string metadataFilename, string responseFilename)
        {
            return new ODataClient(new ODataClientSettings(new Uri("http://localhost/odata"))
            {
                MetadataDocument = MetadataResolver.GetMetadataDocument(metadataFilename),
                IgnoreUnmappedProperties = true
            }.WithHttpResponses(new[] {@"..\..\..\..\..\..\..\Resources\" + responseFilename}));
        }
    }
}
