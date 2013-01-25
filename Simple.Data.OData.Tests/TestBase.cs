using System;
using System.Collections.Generic;
using System.Linq;
using Simple.Data.OData.NorthwindModel;
using Simple.OData.TestUtils;

namespace Simple.Data.OData.Tests
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
            products.ToList().ForEach(x => _db.Products.Delete(ProductID: x.ProductID));
            IEnumerable<dynamic> categories = _db.Categories.FindAll(_db.Categories.CategoryName.StartsWith("Test") == true);
            categories.ToList().ForEach(x => _db.Categories.Delete(CategoryID: x.CategoryID));
            IEnumerable<dynamic> transport = _db.Transport.FindAll(_db.Transports.TransportID > 2);
            transport.ToList().ForEach(x => _db.Transport.Delete(TransportID: x.TransportID));

            if (_service != null)
            {
                _service.Dispose();
                _service = null;
            }
        }
    }
}
