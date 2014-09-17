using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Simple.OData.Client.Tests
{
    public class FindODataTestsV2Atom : FindODataTests
    {
        public FindODataTestsV2Atom() : base(ODataV2ReadWriteUri, ODataPayloadFormat.Atom) { }

        protected override string ProductCategoryName { get { return "Category"; } }
        protected override Func<IDictionary<string, object>, IDictionary<string, object>> ProductCategoryFunc 
        {
            get { return x => x[ProductCategoryName] as IDictionary<string, object>; }
        }
        protected override string ExpectedCategory { get { return "Electronics"; } }
        protected override int ExpectedCount { get { return 9; } }
        protected override int ExpectedExpandMany { get { return 6; } }
        protected override int ExpectedSkipOne { get { return 8; } }
        protected override int ExpectedTotalCount { get { return 9; } }
    }

    public class FindODataTestsV2Json : FindODataTests
    {
        public FindODataTestsV2Json() : base(ODataV2ReadWriteUri, ODataPayloadFormat.Json) { }

        protected override string ProductCategoryName { get { return "Category"; } }
        protected override Func<IDictionary<string, object>, IDictionary<string, object>> ProductCategoryFunc
        {
            get { return x => x[ProductCategoryName] as IDictionary<string, object>; }
        }
        protected override string ExpectedCategory { get { return "Electronics"; } }
        protected override int ExpectedCount { get { return 9; } }
        protected override int ExpectedExpandMany { get { return 6; } }
        protected override int ExpectedSkipOne { get { return 8; } }
        protected override int ExpectedTotalCount { get { return 9; } }
    }

    public class FindODataTestsV3Atom : FindODataTests
    {
        public FindODataTestsV3Atom() : base(ODataV3ReadOnlyUri, ODataPayloadFormat.Atom) { }

        protected override string ProductCategoryName { get { return "Categories"; } }
        protected override Func<IDictionary<string, object>, IDictionary<string, object>> ProductCategoryFunc
        {
            get { return x => (x[ProductCategoryName] as IEnumerable<object>).First() as IDictionary<string, object>; }
        }
        protected override string ExpectedCategory { get { return "Beverages"; } }
        protected override int ExpectedCount { get { return 11; } }
        protected override int ExpectedExpandMany { get { return 8; } }
        protected override int ExpectedSkipOne { get { return 10; } }
        protected override int ExpectedTotalCount { get { return 11; } }
    }

    public class FindODataTestsV3Json : FindODataTests
    {
        public FindODataTestsV3Json() : base(ODataV3ReadOnlyUri, ODataPayloadFormat.Json) { }

        protected override string ProductCategoryName { get { return "Categories"; } }
        protected override Func<IDictionary<string, object>, IDictionary<string, object>> ProductCategoryFunc
        {
            get { return x => (x[ProductCategoryName] as IEnumerable<object>).First() as IDictionary<string, object>; }
        }
        protected override string ExpectedCategory { get { return "Beverages"; } }
        protected override int ExpectedCount { get { return 11; } }
        protected override int ExpectedExpandMany { get { return 8; } }
        protected override int ExpectedSkipOne { get { return 10; } }
        protected override int ExpectedTotalCount { get { return 11; } }
    }

    public class FindODataTestsV4Json : FindODataTests
    {
        public FindODataTestsV4Json() : base(ODataV4ReadOnlyUri, ODataPayloadFormat.Json) { }

        protected override string ProductCategoryName { get { return "Categories"; } }
        protected override Func<IDictionary<string, object>, IDictionary<string, object>> ProductCategoryFunc
        {
            get { return x => (x[ProductCategoryName] as IEnumerable<object>).First() as IDictionary<string, object>; }
        }
        protected override string ExpectedCategory { get { return "Beverages"; } }
        protected override int ExpectedCount { get { return 11; } }
        protected override int ExpectedExpandMany { get { return 8; } }
        protected override int ExpectedSkipOne { get { return 10; } }
        protected override int ExpectedTotalCount { get { return 11; } }
    }

    public abstract class FindODataTests : ODataTests
    {
        protected FindODataTests(string serviceUri, ODataPayloadFormat payloadFormat) : base(serviceUri, payloadFormat) { }

        protected abstract string ProductCategoryName { get; }
        protected abstract Func<IDictionary<string, object>, IDictionary<string, object>> ProductCategoryFunc { get; }
        protected abstract string ExpectedCategory { get; }
        protected abstract int ExpectedCount { get; }
        protected abstract int ExpectedExpandMany { get; }
        protected abstract int ExpectedSkipOne { get; }
        protected abstract int ExpectedTotalCount { get; }

        [Fact]
        public async Task Filter()
        {
            var products = await _client
                .For("Products")
                .Filter("Name eq 'Milk'")
                .FindEntriesAsync();
            Assert.Equal("Milk", products.Single()["Name"]);
        }

        [Fact]
        public async Task FilterStringExpression()
        {
            var x = ODataDynamic.Expression;
            var products = await _client
                .For(x.Products)
                .Filter(x.Name.Contains("lk"))
                .FindEntriesAsync();
            Assert.Equal("Milk", (products as IEnumerable<dynamic>).Single()["Name"]);
        }

        [Fact]
        public async Task Get()
        {
            var category = await _client
                .For("Categories")
                .Key(1)
                .FindEntryAsync();
            Assert.Equal(1, category["ID"]);
        }

        [Fact]
        public async Task GetNonExisting()
        {
            await AssertThrowsAsync<WebRequestException>(async () => await _client
                .For("Categories")
                .Key(-1)
                .FindEntryAsync());
        }

        [Fact]
        public async Task SkipOne()
        {
            var products = await _client
                .For("Products")
                .Skip(1)
                .FindEntriesAsync();
            Assert.Equal(ExpectedSkipOne, products.Count());
        }

        [Fact]
        public async Task TopOne()
        {
            var products = await _client
                .For("Products")
                .Top(1)
                .FindEntriesAsync();
            Assert.Equal(1, products.Count());
        }

        [Fact]
        public async Task SkipOneTopOne()
        {
            var products = await _client
                .For("Products")
                .Skip(1)
                .Top(1)
                .FindEntriesAsync();
            Assert.Equal(1, products.Count());
        }

        [Fact]
        public async Task OrderBy()
        {
            var product = (await _client
                .For("Products")
                .OrderBy("Name")
                .FindEntriesAsync()).First();
            Assert.Equal("Bread", product["Name"]);
        }

        [Fact]
        public async Task OrderByDescending()
        {
            var product = (await _client
                .For("Products")
                .OrderByDescending("Name")
                .FindEntriesAsync()).First();
            Assert.Equal("Vint soda", product["Name"]);
        }

        [Fact]
        public async Task SelectSingle()
        {
            var product = await _client
                .For("Products")
                .Select("Name")
                .FindEntryAsync();
            Assert.Contains("Name", product.Keys);
            Assert.DoesNotContain("ID", product.Keys);
        }

        [Fact]
        public async Task SelectMultiple()
        {
            var product = await _client
                .For("Products")
                .Select("ID", "Name")
                .FindEntryAsync();
            Assert.Contains("Name", product.Keys);
            Assert.Contains("ID", product.Keys);
        }

        [Fact]
        public async Task ExpandOne()
        {
            var product = (await _client
                .For("Products")
                .OrderBy("ID")
                .Expand(ProductCategoryName)
                .FindEntriesAsync()).Last();
            Assert.Equal(ExpectedCategory, ProductCategoryFunc(product)["Name"]);
        }

        [Fact]
        public async Task ExpandMany()
        {
            var category = await _client
                .For("Categories")
                .Expand("Products")
                .Filter("Name eq 'Beverages'")
                .FindEntryAsync();
            Assert.Equal(ExpectedExpandMany, (category["Products"] as IEnumerable<object>).Count());
        }

        [Fact]
        public async Task ExpandSecondLevel()
        {
            var product = (await _client
                .For("Products")
                .OrderBy("ID")
                .Expand(ProductCategoryName + "/Products")
                .FindEntriesAsync()).Last();
            Assert.Equal(8, (ProductCategoryFunc(product)["Products"] as IEnumerable<object>).Count());
        }

        [Fact]
        public async Task Count()
        {
            var count = await _client
                .For("Products")
                .Count()
                .FindScalarAsync();
            Assert.Equal(ExpectedCount, int.Parse(count.ToString()));
        }

        [Fact]
        public async Task FilterCount()
        {
            var count = await _client
                .For("Products")
                .Filter("Name eq 'Milk'")
                .Count()
                .FindScalarAsync();
            Assert.Equal(1, int.Parse(count.ToString()));
        }

        [Fact]
        public async Task TotalCount()
        {
            var productsWithCount = await _client
                .For("Products")
                .FindEntriesWithCountAsync(true);
            Assert.Equal(ExpectedTotalCount, productsWithCount.Item2);
            Assert.Equal(ExpectedTotalCount, productsWithCount.Item1.Count());
        }

        [Fact]
        public async Task CombineAll()
        {
            var product = (await _client
                .For("Products")
                .OrderBy("Name")
                .Skip(2)
                .Top(1)
                .Expand(ProductCategoryName)
                .Select(ProductCategoryName)
                .FindEntriesAsync()).Single();
            Assert.Equal(ExpectedCategory, ProductCategoryFunc(product)["Name"]);
        }

        [Fact]
        public async Task CombineAllReverse()
        {
            var product = (await _client
                .For("Products")
                .Select(ProductCategoryName)
                .Expand(ProductCategoryName)
                .Top(1)
                .Skip(2)
                .OrderBy("Name")
                .FindEntriesAsync()).Single();
            Assert.Equal(ExpectedCategory, ProductCategoryFunc(product)["Name"]);
        }

        [Fact]
        public async Task NavigateToSingle()
        {
            var category = await _client
                .For("Products")
                .Key(new Dictionary<string, object>() { { "ID", 2 } })
                .NavigateTo(ProductCategoryName)
                .FindEntryAsync();
            Assert.Equal("Beverages", category["Name"]);
        }

        [Fact]
        public async Task NavigateToMultiple()
        {
            var products = await _client
                .For("Categories")
                .Key(2)
                .NavigateTo("Products")
                .FindEntriesAsync();
            Assert.Equal(2, products.Count());
        }
    }
}