using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using Entry = System.Collections.Generic.Dictionary<string, object>;

namespace Simple.OData.Client.Tests
{
    public class FindTests : TestBase
    {
        public FindTests()
            : base(true)
        {
        }

        [Fact]
        public async Task Filter()
        {
            var products = await FindEntriesAsync(_client
                .For("Products")
                .Filter("ProductName eq 'Chai'"));
            Assert.Equal("Chai", products.Single()["ProductName"]);
        }

        [Fact]
        public async Task FilterStringExpression()
        {
            var products = await FindEntriesAsync(_client
                .For("Products")
                .Filter("substringof('ai',ProductName)"));
            Assert.Equal("Chai", products.Single()["ProductName"]);
        }

        [Fact]
        public async Task Get()
        {
            var category = await FindEntryAsync(_client
                .For("Categories")
                .Key(1));
            Assert.Equal(1, category["CategoryID"]);
        }

        [Fact]
        public async Task GetNonExisting()
        {
            await AssertThrowsAsync<WebRequestException>(async () => await FindEntryAsync(_client
                .For("Categories")
                .Key(-1)));
        }

        [Fact]
        public async Task SkipOne()
        {
            var products = await FindEntriesAsync(_client
                .For("Products")
                .Skip(1));
            Assert.Equal(ExpectedCountOfProducts-1, products.Count());
        }

        [Fact]
        public async Task TopOne()
        {
            var products = await FindEntriesAsync(_client
                .For("Products")
                .Top(1));
            Assert.Single(products);
        }

        [Fact]
        public async Task SkipOneTopOne()
        {
            var products = await FindEntriesAsync(_client
                .For("Products")
                .Skip(1)
                .Top(1));
            Assert.Single(products);
        }

        [Fact]
        public async Task OrderBy()
        {
            var product = (await FindEntriesAsync(_client
                .For("Products")
                .OrderBy("ProductName"))).First();
            Assert.Equal("Alice Mutton", product["ProductName"]);
        }

        [Fact]
        public async Task OrderByDescending()
        {
            var product = (await FindEntriesAsync(_client
                .For("Products")
                .OrderByDescending("ProductName"))).First();
            Assert.Equal("Zaanse koeken", product["ProductName"]);
        }

        [Fact]
        public async Task OrderByExpanded()
        {
            var product = (await FindEntriesAsync(_client
                .For("Products")
                .Expand("Category")
                .OrderBy("Category/CategoryName"))).Last();
            Assert.Equal("Seafood", (product["Category"] as IDictionary<string, object>)["CategoryName"]);
        }

        [Fact]
        public async Task SelectSingle()
        {
            var product = await FindEntryAsync(_client
                .For("Products")
                .Select("ProductName"));
            Assert.Contains("ProductName", product.Keys);
            Assert.DoesNotContain("ProductID", product.Keys);
        }

        [Fact]
        public async Task SelectSingleHomogenize()
        {
            var product = await FindEntryAsync(_client
                .For("Products")
                .Select("Product_Name"));
            Assert.Contains("ProductName", product.Keys);
            Assert.DoesNotContain("ProductID", product.Keys);
        }

        [Fact]
        public async Task SelectMultiple()
        {
            var product = await FindEntryAsync(_client
                .For("Products")
                .Select("ProductID", "ProductName"));
            Assert.Contains("ProductName", product.Keys);
            Assert.Contains("ProductID", product.Keys);
        }

        [Fact]
        public async Task SelectMultipleSingleString()
        {
            var product = await FindEntryAsync(_client
                .For("Products")
                .Select("ProductID, ProductName"));
            Assert.Equal(2, product.Count);
            Assert.Contains("ProductName", product.Keys);
            Assert.Contains("ProductID", product.Keys);
        }

        [Fact]
        public async Task ExpandOne()
        {
            var product = (await FindEntriesAsync(_client
                .For("Products")
                .OrderBy("ProductID")
                .Expand("Category"))).Last();
            Assert.Equal("Condiments", (product["Category"] as IDictionary<string, object>)["CategoryName"]);
        }

        [Fact]
        public async Task ExpandMany()
        {
            var category = await FindEntryAsync(_client
                .For("Categories")
                .Expand("Products")
                .Filter("CategoryName eq 'Beverages'"));
            Assert.Equal(ExpectedCountOfBeveragesProducts, (category["Products"] as IEnumerable<object>).Count());
        }

        [Fact]
        public async Task ExpandSecondLevel()
        {
            var product = (await FindEntriesAsync(_client
                .For("Products")
                .OrderBy("ProductID")
                .Expand("Category/Products"))).Last();
            Assert.Equal(ExpectedCountOfCondimentsProducts, ((product["Category"] as IDictionary<string, object>)["Products"] as IEnumerable<object>).Count());
        }

        [Fact]
        public async Task ExpandMultipleLevelsWithCollection()
        {
            var product = (await FindEntriesAsync(_client
                .For("Products")
                .OrderBy("ProductID")
                .Expand("Category/Products/Category"))).Last();
            Assert.Equal("Condiments", ((((product["Category"] as IDictionary<string, object>)["Products"] as IEnumerable<object>)
                                        .First() as IDictionary<string, object>)["Category"] as IDictionary<string, object>)["CategoryName"]);
        }

        [Fact]
        public async Task ExpandMultipleLevelsWithCollectionAndSelect()
        {
            var product = (await FindEntriesAsync(_client
                .For("Products")
                .OrderBy("ProductID")
                .Expand("Category/Products/Category")
                .Select(new[] { "Category/Products/Category/CategoryName" }))).Last();
            Assert.Equal(1, product.Count);
            Assert.Equal(1, (product["Category"] as IDictionary<string, object>).Count);
            Assert.Equal(1, (((product["Category"] as IDictionary<string, object>)["Products"] as IEnumerable<object>)
                                        .First() as IDictionary<string, object>).Count);
            Assert.Equal(1, ((((product["Category"] as IDictionary<string, object>)["Products"] as IEnumerable<object>)
                                        .First() as IDictionary<string, object>)["Category"] as IDictionary<string, object>).Count);
            Assert.Equal("Condiments", ((((product["Category"] as IDictionary<string, object>)["Products"] as IEnumerable<object>)
                                        .First() as IDictionary<string, object>)["Category"] as IDictionary<string, object>)["CategoryName"]);
        }

        [Fact]
        public async Task ExpandWithSelect()
        {
            var product = (await FindEntriesAsync(_client
                .For("Products")
                .OrderBy("ProductID")
                .Expand("Category/Products")
                .Select(new[] {"ProductName", "Category/CategoryName" }))).Last();
            Assert.Equal(2, product.Count);
            Assert.Equal(1, (product["Category"] as IDictionary<string, object>).Count);
            Assert.Equal("Condiments", (product["Category"] as IDictionary<string, object>)["CategoryName"]);
        }

        [Fact]
        public async Task Count()
        {
            var count = await FindScalarAsync<IDictionary<string, object>, int>(_client
                .For("Products")
                .Count());
            Assert.Equal(ExpectedCountOfProducts, count);
        }

        [Fact]
        public async Task FilterCount()
        {
            var count = await FindScalarAsync<IDictionary<string,object>, int>(_client
                .For("Products")
                .Filter("ProductName eq 'Chai'")
                .Count());
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task TotalCount()
        {
            var annotations = new ODataFeedAnnotations();
            var products = await FindEntriesAsync(_client
                .For("Products"), annotations);
            Assert.Equal(ExpectedCountOfProducts, annotations.Count);
            Assert.Equal(ExpectedCountOfProducts, products.Count());
        }

        [Fact]
        public async Task CombineAll()
        {
            var product = (await FindEntriesAsync(_client
                .For("Products")
                .OrderBy("ProductName")
                .Skip(2)
                .Top(1)
                .Expand("Category")
                .Select("Category"))).Single();
            Assert.Equal("Seafood", (product["Category"] as IDictionary<string, object>)["CategoryName"]);
        }

        [Fact]
        public async Task CombineAllReverse()
        {
            var product = (await FindEntriesAsync(_client
                .For("Products")
                .Select("Category")
                .Expand("Category")
                .Top(1)
                .Skip(2)
                .OrderBy("ProductName"))).Single();
            Assert.Equal("Seafood", (product["Category"] as IDictionary<string, object>)["CategoryName"]);
        }

        [Fact]
        public async Task NavigateToSingle()
        {
            var category = await FindEntryAsync(_client
                .For("Products")
                .Key(new Entry() { { "ProductID", 2 } })
                .NavigateTo("Category"));
            Assert.Equal("Beverages", category["CategoryName"]);
        }

        [Fact]
        public async Task NavigateToMultiple()
        {
            var products = await FindEntriesAsync(_client
                .For("Categories")
                .Key(2)
                .NavigateTo("Products"));
            Assert.Equal(ExpectedCountOfCondimentsProducts, products.Count());
        }

        [Fact]
        public async Task NavigateToRecursive()
        {
            var employee = await FindEntryAsync(_client
                .For("Employees")
                .Key(14)
                .NavigateTo("Superior")
                .NavigateTo("Superior")
                .NavigateTo("Subordinates")
                .Key(3));
            Assert.Equal("Janet", employee["FirstName"]);
        }

        [Fact]
        public async Task NavigateToRecursiveSingleClause()
        {
            var employee = await FindEntryAsync(_client
                .For("Employees")
                .Key(14)
                .NavigateTo("Superior/Superior/Subordinates")
                .Key(3));
            Assert.Equal("Janet", employee["FirstName"]);
        }

        [Fact]
        public async Task BaseClassEntries()
        {
            var transport = await FindEntriesAsync(_client
                .For("Transport"));
            Assert.Equal(2, transport.Count());
            Assert.DoesNotContain(transport, x => x.ContainsKey(FluentCommand.AnnotationsLiteral));
        }

        [Fact]
        public async Task BaseClassEntriesWithAnnotations()
        {
            var clientSettings = new ODataClientSettings
                                     {
                                         BaseUri = _serviceUri,
                                         IncludeAnnotationsInResults = true,
                                     };
            var client = new ODataClient(clientSettings);
            var transport = await FindEntriesAsync(client
                .For("Transport"));
            Assert.Equal(2, transport.Count());
            Assert.True(transport.All(x => x.ContainsKey(FluentCommand.AnnotationsLiteral)));
        }

        [Fact]
        public async Task AllDerivedClassEntries()
        {
            var transport = await FindEntriesAsync(_client
                .For("Transport")
                .As("Ships"));
            Assert.Equal("Titanic", transport.Single()["ShipName"]);
        }

        [Fact]
        public async Task AllDerivedClassEntriesWithAnnotations()
        {
            var clientSettings = new ODataClientSettings
            {
                BaseUri = _serviceUri,
                IncludeAnnotationsInResults = true,
            };
            var client = new ODataClient(clientSettings);
            var transport = await FindEntriesAsync(client
                .For("Transport")
                .As("Ships"));
            Assert.Equal("Titanic", transport.Single()["ShipName"]);
            Assert.Equal("NorthwindModel.Ship", (transport.Single()[FluentCommand.AnnotationsLiteral] as ODataEntryAnnotations).TypeName);
        }

        [Fact]
        public async Task DerivedClassEntry()
        {
            var transport = await FindEntryAsync(_client
                .For("Transport")
                .As("Ships")
                .Filter("ShipName eq 'Titanic'"));
            Assert.Equal("Titanic", transport["ShipName"]);
        }

        [Fact]
        public async Task BaseClassEntryByKey()
        {
            var transport = await FindEntryAsync(_client
                .For("Transport")
                .Key(1));
            Assert.Equal("Titanic", transport["ShipName"]);
        }

        [Fact]
        public async Task DerivedClassEntryByKey()
        {
            var transport = await FindEntryAsync(_client
                .For("Transport")
                .As("Ships")
                .Key(1));
            Assert.Equal("Titanic", transport["ShipName"]);
        }

        [Fact]
        public async Task DerivedClassEntryBaseAndDerivedFields()
        {
            var transport = await FindEntryAsync(_client
                .For("Transport")
                .As("Ships")
                .Filter("TransportID eq 1 and ShipName eq 'Titanic'"));
            Assert.Equal("Titanic", transport["ShipName"]);
        }

        [Fact]
        public async Task IsOfDerivedClassEntry()
        {
            var transport = await FindEntryAsync(_client
                .For("Transport")
                .Filter("isof('NorthwindModel.Ship')"));
            Assert.Equal("Titanic", transport["ShipName"]);
        }

        [Fact]
        public async Task IsOfAssociation()
        {
            var employee = await FindEntryAsync(_client
                .For("Employees")
                .Filter("isof(Superior, 'NorthwindModel.Employee')"));
            Assert.NotNull(employee);
        }

        [Fact]
        public async Task CastToPrimitiveType()
        {
            var product = await FindEntryAsync(_client
                .For("Products")
                .Filter("ProductID eq cast(1L, 'Edm.Int32')"));
            Assert.NotNull(product);
        }

        [Fact]
        public async Task CastInstanceToEntityType()
        {
            var employee = await FindEntryAsync(_client
                .For("Employees")
                .Filter("cast('NorthwindModel.Employee') ne null"));
            Assert.NotNull(employee);
        }

        [Fact]
        public async Task CastPropertyToEntityType()
        {
            var employee = await FindEntryAsync(_client
                .For("Employees")
                .Filter("cast(Superior, 'NorthwindModel.Employee') ne null"));
            Assert.NotNull(employee);
        }

        [Fact]
        public async Task FilterAny()
        {
            var products = await FindEntriesAsync(_client
                .For("Orders")
                .Filter("Order_Details/any(d:d/Quantity gt 50)"));
            Assert.Equal(ExpectedCountOfOrdersHavingAnyDetail, products.Count());
        }

        [Fact]
        public async Task FilterAll()
        {
            var products = await FindEntriesAsync(_client
                .For("Orders")
                .Filter("Order_Details/all(d:d/Quantity gt 50)"));
            Assert.Equal(ExpectedCountOfOrdersHavingAllDetails, products.Count());
        }
    }
}
