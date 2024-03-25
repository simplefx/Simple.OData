using FluentAssertions;
using Xunit;
using Entry = System.Collections.Generic.Dictionary<string, object>;

namespace Simple.OData.Client.Tests.FluentApi;

public class FindTests : TestBase
{
	public FindTests()
		: base(true)
	{
	}

	[Fact]
	public async Task Filter()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var products = await client
			.For("Products")
			.Filter("ProductName eq 'Chai'")
			.FindEntriesAsync();
		products.Single()["ProductName"].Should().Be("Chai");
	}

	[Fact]
	public async Task FilterStringExpression()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var products = await client
			.For("Products")
			.Filter("substringof('ai',ProductName)")
			.FindEntriesAsync();
		products.Single()["ProductName"].Should().Be("Chai");
	}

	[Fact]
	public async Task Get()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var category = await client
			.For("Categories")
			.Key(1)
			.FindEntryAsync();
		category["CategoryID"].Should().Be(1);
	}

	[Fact]
	public async Task GetNonExisting()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		await AssertThrowsAsync<WebRequestException>(async () => await client
			.For("Categories")
			.Key(-1)
			.FindEntryAsync());
	}

	[Fact]
	public async Task SkipOne()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var products = await client
			.For("Products")
			.Skip(1)
			.FindEntriesAsync();
		Assert.Equal(ExpectedCountOfProducts - 1, products.Count());
	}

	[Fact]
	public async Task TopOne()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var products = await client
			.For("Products")
			.Top(1)
			.FindEntriesAsync();
		Assert.Single(products);
	}

	[Fact]
	public async Task SkipOneTopOne()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var products = await client
			.For("Products")
			.Skip(1)
			.Top(1)
			.FindEntriesAsync();
		Assert.Single(products);
	}

	[Fact]
	public async Task OrderBy()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var product = (await client
			.For("Products")
			.OrderBy("ProductName")
			.FindEntriesAsync()).First();
		product["ProductName"].Should().Be("Alice Mutton");
	}

	[Fact]
	public async Task OrderByDescending()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var product = (await client
			.For("Products")
			.OrderByDescending("ProductName")
			.FindEntriesAsync()).First();
		product["ProductName"].Should().Be("Zaanse koeken");
	}

	[Fact]
	public async Task OrderByExpanded()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var product = (await client
			.For("Products")
			.Expand("Category")
			.OrderBy("Category/CategoryName")
			.FindEntriesAsync()).Last();
		(product["Category"] as IDictionary<string, object>)["CategoryName"].Should().Be("Seafood");
	}

	[Fact]
	public async Task SelectSingle()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var product = await client
			.For("Products")
			.Select("ProductName")
			.FindEntryAsync();
		product.Keys.Should().Contain("ProductName");
		product.Keys.Should().NotContain("ProductID");
	}

	[Fact]
	public async Task SelectSingleHomogenize()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var product = await client
			.For("Products")
			.Select("Product_Name")
			.FindEntryAsync();
		product.Keys.Should().Contain("ProductName");
		product.Keys.Should().NotContain("ProductID");
	}

	[Fact]
	public async Task SelectMultiple()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var product = await client
			.For("Products")
			.Select("ProductID", "ProductName")
			.FindEntryAsync();
		product.Keys.Should().Contain("ProductName");
		product.Keys.Should().Contain("ProductID");
	}

	[Fact]
	public async Task SelectMultipleSingleString()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var product = await client
			.For("Products")
			.Select("ProductID, ProductName")
			.FindEntryAsync();
		Assert.Equal(2, product.Count);
		product.Keys.Should().Contain("ProductName");
		product.Keys.Should().Contain("ProductID");
	}

	[Fact]
	public async Task ExpandOne()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var product = (await client
			.For("Products")
			.OrderBy("ProductID")
			.Expand("Category")
			.FindEntriesAsync()).Last();
		(product["Category"] as IDictionary<string, object>)["CategoryName"].Should().Be("Condiments");
	}

	[Fact]
	public async Task ExpandMany()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var category = await client
			.For("Categories")
			.Expand("Products")
			.Filter("CategoryName eq 'Beverages'")
			.FindEntryAsync();
		Assert.Equal(ExpectedCountOfBeveragesProducts, (category["Products"] as IEnumerable<object>).Count());
	}

	[Fact]
	public async Task ExpandSecondLevel()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var product = (await client
			.For("Products")
			.OrderBy("ProductID")
			.Expand("Category/Products")
			.FindEntriesAsync()).Last();
		Assert.Equal(ExpectedCountOfCondimentsProducts, ((product["Category"] as IDictionary<string, object>)["Products"] as IEnumerable<object>).Count());
	}

	[Fact]
	public async Task ExpandMultipleLevelsWithCollection()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var product = (await client
			.For("Products")
			.OrderBy("ProductID")
			.Expand("Category/Products/Category")
			.FindEntriesAsync()).Last();
		((((product["Category"] as IDictionary<string, object>)["Products"] as IEnumerable<object>)
									.First() as IDictionary<string, object>)["Category"] as IDictionary<string, object>)["CategoryName"].Should().Be("Condiments");
	}

	[Fact]
	public async Task ExpandMultipleLevelsWithCollectionAndSelect()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var product = (await client
			.For("Products")
			.OrderBy("ProductID")
			.Expand("Category/Products/Category")
			.Select(new[] { "Category/Products/Category/CategoryName" })
			.FindEntriesAsync()).Last();
		Assert.Equal(1, product.Count);
		Assert.Equal(1, (product["Category"] as IDictionary<string, object>).Count);
		Assert.Equal(1, (((product["Category"] as IDictionary<string, object>)["Products"] as IEnumerable<object>)
									.First() as IDictionary<string, object>).Count);
		Assert.Equal(1, ((((product["Category"] as IDictionary<string, object>)["Products"] as IEnumerable<object>)
									.First() as IDictionary<string, object>)["Category"] as IDictionary<string, object>).Count);
		((((product["Category"] as IDictionary<string, object>)["Products"] as IEnumerable<object>)
									.First() as IDictionary<string, object>)["Category"] as IDictionary<string, object>)["CategoryName"].Should().Be("Condiments");
	}

	[Fact]
	public async Task ExpandWithSelect()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var product = (await client
			.For("Products")
			.OrderBy("ProductID")
			.Expand("Category/Products")
			.Select(new[] { "ProductName", "Category/CategoryName" })
			.FindEntriesAsync()).Last();
		Assert.Equal(2, product.Count);
		Assert.Equal(1, (product["Category"] as IDictionary<string, object>).Count);
		(product["Category"] as IDictionary<string, object>)["CategoryName"].Should().Be("Condiments");
	}

	[Fact]
	public async Task Count()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var count = await client
			.For("Products")
			.Count()
			.FindScalarAsync<int>();
		Assert.Equal(ExpectedCountOfProducts, count);
	}

	[Fact]
	public async Task FilterCount()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var count = await client
			.For("Products")
			.Filter("ProductName eq 'Chai'")
			.Count()
			.FindScalarAsync<int>();
		Assert.Equal(1, count);
	}

	[Fact]
	public async Task TotalCount()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var annotations = new ODataFeedAnnotations();
		var products = await client
			.For("Products")
			.FindEntriesAsync(annotations);
		Assert.Equal(ExpectedCountOfProducts, annotations.Count);
		Assert.Equal(ExpectedCountOfProducts, products.Count());
	}

	[Fact]
	public async Task CombineAll()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var product = (await client
			.For("Products")
			.OrderBy("ProductName")
			.Skip(2)
			.Top(1)
			.Expand("Category")
			.Select("Category")
			.FindEntriesAsync()).Single();
		(product["Category"] as IDictionary<string, object>)["CategoryName"].Should().Be("Seafood");
	}

	[Fact]
	public async Task CombineAllReverse()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var product = (await client
			.For("Products")
			.Select("Category")
			.Expand("Category")
			.Top(1)
			.Skip(2)
			.OrderBy("ProductName")
			.FindEntriesAsync()).Single();
		(product["Category"] as IDictionary<string, object>)["CategoryName"].Should().Be("Seafood");
	}

	[Fact]
	public async Task NavigateToSingle()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var category = await client
			.For("Products")
			.Key(new Entry() { { "ProductID", 2 } })
			.NavigateTo("Category")
			.FindEntryAsync();
		category["CategoryName"].Should().Be("Beverages");
	}

	[Fact]
	public async Task NavigateToMultiple()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var products = await client
			.For("Categories")
			.Key(2)
			.NavigateTo("Products")
			.FindEntriesAsync();
		Assert.Equal(ExpectedCountOfCondimentsProducts, products.Count());
	}

	[Fact]
	public async Task NavigateToRecursive()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var employee = await client
			.For("Employees")
			.Key(14)
			.NavigateTo("Superior")
			.NavigateTo("Superior")
			.NavigateTo("Subordinates")
			.Key(3)
			.FindEntryAsync();
		employee["FirstName"].Should().Be("Janet");
	}

	[Fact]
	public async Task NavigateToRecursiveSingleClause()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var employee = await client
			.For("Employees")
			.Key(14)
			.NavigateTo("Superior/Superior/Subordinates")
			.Key(3)
			.FindEntryAsync();
		employee["FirstName"].Should().Be("Janet");
	}

	[Fact]
	public async Task BaseClassEntries()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var transport = await client
			.For("Transport")
			.FindEntriesAsync();
		Assert.Equal(2, transport.Count());
		(x => x.ContainsKey(FluentCommand.AnnotationsLiteral)).Should().NotContain(transport);
	}

	[Fact]
	public async Task BaseClassEntriesWithAnnotations()
	{
		var client = new ODataClient(CreateDefaultSettings().WithAnnotations().WithHttpMock());
		var transport = await client
			.For("Transport")
			.FindEntriesAsync();
		Assert.Equal(2, transport.Count());
		transport.All(x => x.ContainsKey(FluentCommand.AnnotationsLiteral)).Should().BeTrue();
	}

	[Fact]
	public async Task AllDerivedClassEntries()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var transport = await client
			.For("Transport")
			.As("Ships")
			.FindEntriesAsync();
		transport.Single()["ShipName"].Should().Be("Titanic");
	}

	[Fact]
	public async Task AllDerivedClassEntriesWithAnnotations()
	{
		var client = new ODataClient(CreateDefaultSettings().WithAnnotations().WithHttpMock());
		var transport = await client
			.For("Transport")
			.As("Ships")
			.FindEntriesAsync();

		var ship = transport.Single();
		ship["ShipName"].Should().Be("Titanic");
		(ship[FluentCommand.AnnotationsLiteral] as ODataEntryAnnotations).TypeName.Should().Be("NorthwindModel.Ship");
	}

	[Fact]
	public async Task DerivedClassEntry()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var transport = await client
			.For("Transport")
			.As("Ships")
			.Filter("ShipName eq 'Titanic'")
			.FindEntryAsync();
		transport["ShipName"].Should().Be("Titanic");
	}

	[Fact]
	public async Task BaseClassEntryByKey()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var transport = await client
			.For("Transport")
			.Key(1)
			.FindEntryAsync();
		transport["ShipName"].Should().Be("Titanic");
	}

	[Fact]
	public async Task DerivedClassEntryByKey()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var transport = await client
			.For("Transport")
			.As("Ships")
			.Key(1)
			.FindEntryAsync();
		transport["ShipName"].Should().Be("Titanic");
	}

	[Fact]
	public async Task DerivedClassEntryBaseAndDerivedFields()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var transport = await client
			.For("Transport")
			.As("Ships")
			.Filter("TransportID eq 1 and ShipName eq 'Titanic'")
			.FindEntryAsync();
		transport["ShipName"].Should().Be("Titanic");
	}

	[Fact]
	public async Task IsOfDerivedClassEntry()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var transport = await client
			.For("Transport")
			.Filter("isof('NorthwindModel.Ship')")
			.FindEntryAsync();
		transport["ShipName"].Should().Be("Titanic");
	}

	[Fact]
	public async Task IsOfAssociation()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var employee = await client
			.For("Employees")
			.Filter("isof(Superior, 'NorthwindModel.Employee')")
			.FindEntryAsync();
		employee.Should().NotBeNull();
	}

	[Fact]
	public async Task CastToPrimitiveType()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var product = await client
			.For("Products")
			.Filter("ProductID eq cast(1L, 'Edm.Int32')")
			.FindEntryAsync();
		product.Should().NotBeNull();
	}

	[Fact]
	public async Task CastInstanceToEntityType()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var employee = await client
			.For("Employees")
			.Filter("cast('NorthwindModel.Employee') ne null")
			.FindEntryAsync();
		employee.Should().NotBeNull();
	}

	[Fact]
	public async Task CastPropertyToEntityType()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var employee = await client
			.For("Employees")
			.Filter("cast(Superior, 'NorthwindModel.Employee') ne null")
			.FindEntryAsync();
		employee.Should().NotBeNull();
	}

	[Fact]
	public async Task FilterAny()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var products = await client
			.For("Orders")
			.Filter("Order_Details/any(d:d/Quantity gt 50)")
			.FindEntriesAsync();
		Assert.Equal(ExpectedCountOfOrdersHavingAnyDetail, products.Count());
	}

	[Fact]
	public async Task FilterAll()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var products = await client
			.For("Orders")
			.Filter("Order_Details/all(d:d/Quantity gt 50)")
			.FindEntriesAsync();
		Assert.Equal(ExpectedCountOfOrdersHavingAllDetails, products.Count());
	}
}
