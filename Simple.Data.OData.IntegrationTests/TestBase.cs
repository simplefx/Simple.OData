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
            products.ToList().ForEach(x => _db.Products.Delete(ProductID: x.ProductID));
            IEnumerable<dynamic> categories = _db.Categories.FindAll(_db.Categories.CategoryName.StartsWith("Test") == true);
            categories.ToList().ForEach(x => _db.Categories.Delete(CategoryID: x.CategoryID));

            if (_service != null)
            {
                _service.Dispose();
                _service = null;
            }
        }
    }
}
