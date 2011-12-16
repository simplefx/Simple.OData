using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.Azure.Helpers;

namespace Simple.Data.Azure.IntegrationTest
{
    using Xunit;

    public class FindTest
    {
        private string _account = "devstoreaccount1";
        private string _url = "http://localhost:10002/devstoreaccount1/";
        private string _key = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";

        public FindTest()
        {
            CreateTestTableAndData();
        }

        [Fact]
        public void SimpleFindTest()
        {
            var db = Database.Opener.Open("Azure", new { Account = _account, Url = _url, Key = _key });

            var mark = db.SimpleTest.FindByPartitionKey("1");
            Assert.NotNull(mark);
            Assert.Equal("Mark", mark.Name);
            Assert.Equal(25, mark.Age);
        }

        private void CreateTestTableAndData()
        {
            var azureHelper = new ProviderHelper { Account = _account, UrlBase = _url, SharedKey = _key };
            var tableService = new TableService(azureHelper);
            var tables = tableService.ListTables();
            if (!tables.Contains("SimpleTest"))
            {
                var table = new AzureTable("SimpleTest", IfTableDoesNotExist.CreateIt, azureHelper);
                table.InsertRow(new Dictionary<string, object>
                                {
                                    {"PartitionKey", "1"}, 
                                    {"RowKey", Guid.NewGuid()}, 
                                    {"Name", "Mark"}, 
                                    {"Age", 25}
                                });
            }
        }
    }
}
