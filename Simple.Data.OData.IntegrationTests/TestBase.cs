using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.OData.NorthwindModel;

namespace Simple.Data.OData.IntegrationTests
{
    public class TestBase : IDisposable
    {
        protected TestService _service;
        protected dynamic _db;

        public TestBase()
        {
            _service = new TestService(typeof(NorthwindService));
            _db = Database.Opener.Open(_service.ServiceUri);
            Database.SetPluralizer(new EntityPluralizer());
        }

        public void Dispose()
        {
            IEnumerable<dynamic> products = _db.Products.FindAll(_db.Products.ProductName.StartsWith("Test") == true);
            foreach (var product in products)
            {
                _db.Products.Delete(ProductID: product.ProductID);
            }

            if (_service != null)
            {
                _service.Dispose();
                _service = null;
            }
        }
    }
}
