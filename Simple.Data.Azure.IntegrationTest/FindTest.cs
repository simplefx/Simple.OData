using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Azure.IntegrationTest
{
    using Xunit;

    public class FindTest
    {
        [Fact]
        public void SimpleFindTest()
        {
            var db = Database.Opener.Open("Azure",
                new { Account="devstoreaccount1",
                Url = "http://127.0.0.1:10002/devstoreaccount1/",
                Key = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==" });

            var mark = db.SimpleTest.FindByPartitionKey("1");
            Assert.NotNull(mark);
            Assert.Equal("Mark", mark.Name);
            Assert.Equal(25, mark.Age);
        }
    }
}
