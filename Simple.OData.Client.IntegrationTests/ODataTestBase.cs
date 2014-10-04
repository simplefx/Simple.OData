using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Entry = System.Collections.Generic.Dictionary<string, object>;

namespace Simple.OData.Client.Tests
{
    public abstract class ODataTestBase : TestBase
    {
        protected readonly int _version;

        protected ODataTestBase(string serviceUri, ODataPayloadFormat payloadFormat, int version)
            : base(serviceUri, payloadFormat)
        {
            _version = version;
        }

        protected string ProductCategoryName
        {
            get { return _version == 2 ? "Category" : "Categories"; }
        }

        protected Func<IDictionary<string, object>, IDictionary<string, object>> ProductCategoryFunc
        {
            get
            {
                return x => _version == 2 
                ? x[ProductCategoryName] as IDictionary<string, object>
                : (x[ProductCategoryName] as IEnumerable<object>).Last() as IDictionary<string, object>;
            }
        }

        protected Func<IDictionary<string, object>, object> ProductCategoryLinkFunc
        {
            get
            {
                if (_version == 2)
                    return x => x;
                else
                    return x => new List<IDictionary<string, object>>() {x};
            }
        }

        protected string ExpectedCategory
        {
            get { return _version == 2 ? "Electronics" : "Beverages"; }
        }

        protected int ExpectedCount
        {
            get { return _version == 2 ? 9 : 11; }
        }

        protected int ExpectedExpandMany
        {
            get { return _version == 2 ? 6 : 8; }
        }

        protected int ExpectedExpandSecondLevel
        {
            get { return _version == 2 ? 2 : 8; }
        }

        protected int ExpectedSkipOne
        {
            get { return _version == 2 ? 8 : 10; }
        }

        protected int ExpectedTotalCount
        {
            get { return _version == 2 ? 9 : 11; }
        }

        protected Entry CreateProduct(int productId, string productName, IDictionary<string, object> category = null)
        {
            var entry = new Entry()
                {
                    {"ID", productId},
                    {"Name", productName},
                    {"Description", "Test1"},
                    {"Price", 18},
                    {"Rating", 1},
                    {"ReleaseDate", DateTimeOffset.Now},
                };

            if (category != null)
            {
                entry.Add(ProductCategoryName, ProductCategoryLinkFunc(category));
            }
            return entry;
        }

        protected Entry CreateCategory(int categoryId, string categoryName, IEnumerable<IDictionary<string, object>> products = null)
        {
            var entry = new Entry()
            {
                {"ID", categoryId},
                {"Name", categoryName},
            };

            if (products != null)
            {
                entry.Add("Products", products);
            }
            return entry;
        }

        protected override async Task DeleteTestData()
        {
            try
            {
                var products = await _client.For("Products").Select("ID", "Name").FindEntriesAsync();
                foreach (var product in products)
                {
                    if (product["Name"].ToString().StartsWith("Test"))
                        await _client.DeleteEntryAsync("Products", product);
                }
                var categories = await _client.For("Categories").Select("ID", "Name").FindEntriesAsync();
                foreach (var category in categories)
                {
                    if (category["Name"].ToString().StartsWith("Test"))
                        await _client.DeleteEntryAsync("Categories", category);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}