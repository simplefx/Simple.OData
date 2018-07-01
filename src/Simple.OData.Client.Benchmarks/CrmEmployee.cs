using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Simple.OData.Client;
using BenchmarkDotNet.Attributes;
using Simple.OData.Client.Tests;
using Xunit;

namespace Simple.OData.Client.Benchmarks
{
    public class he_employee
    {
        public int he_employeenumber { get; set; }
    }

    public class CrmEmployee
    {
        private ODataClient GetClient(string responseFile)
        {
            return new ODataClient(new ODataClientSettings("http://localhost/odata")
            {
                MetadataDocument = MetadataResolver.GetMetadataDocument("crm_schema.xml"),
                IgnoreUnmappedProperties = true
            }.WithHttpResponses(new [] { @"..\..\..\..\..\..\..\Resources\" + responseFile }));
        }

        [Benchmark]
        public void GetAll()
        {
            var result = GetClient("crm_result_10.json")
                .For<he_employee>()
                .FindEntriesAsync()
                .Result.ToList();
            Assert.Equal(10, result.Count);
        }

        [Benchmark]
        public void GetSingle()
        {
            var result = GetClient("crm_result_1.json")
                .For<he_employee>()
                .Filter(x => x.he_employeenumber == 123456)
                .FindEntryAsync()
                .Result;
            Assert.Equal(123456, result.he_employeenumber);
        }
    }
}
