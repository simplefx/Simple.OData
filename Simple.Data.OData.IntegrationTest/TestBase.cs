using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.OData.IntegrationTest
{
    public class TestBase
    {
        private const string _northwindUrl = "http://services.odata.org/Northwind/Northwind.svc/";
        protected dynamic _db;

        public TestBase()
        {
            _db = Database.Opener.Open(_northwindUrl);
        }

    }
}
