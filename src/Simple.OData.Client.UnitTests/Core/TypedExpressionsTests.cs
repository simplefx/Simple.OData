using System.Linq.Expressions;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Simple.OData.Client.Tests.Core;

public class TypedExpressionV3Tests : TypedExpressionTests
{
	public override string MetadataFile => "Northwind3.xml";
	public override IFormatSettings FormatSettings => new ODataV3Format();
}

public class TypedExpressionV4Tests : TypedExpressionTests
{
	public override string MetadataFile => "Northwind4.xml";
	public override IFormatSettings FormatSettings => new ODataV4Format();

	[Fact]
	public void FilterEntitiesWithContains()
	{
		var ids = new List<int> { 1, 2, 3 };
		Expression<Func<TestEntity, bool>> filter = x => ids.Contains(x.ProductID);
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("(ProductID in (1,2,3))");
	}

	[Fact]
	public void FilterEntitiesByStringPropertyWithContains()
	{
		var names = new List<string> { "Chai", "Milk", "Water" };
		Expression<Func<TestEntity, bool>> filter = x => names.Contains(x.ProductName);
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("(ProductName in ('Chai','Milk','Water'))");
	}

	[Fact]
	public void FilterEntitiesByNestedPropertyWithContains()
	{
		var categories = new List<string> { "Chai", "Milk", "Water" };
		Expression<Func<TestEntity, bool>> filter = x => categories.Contains(x.Nested.ProductName);
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("(Nested/ProductName in ('Chai','Milk','Water'))");
	}

	[Fact]
	public void FilterEntitiesByComplexConditionWithContains()
	{
		var categories = new List<string> { "chai", "milk", "water" };
		Expression<Func<TestEntity, bool>> filter = x => categories.Contains(x.ProductName.ToLower());
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("(tolower(ProductName) in ('chai','milk','water'))");
	}

	[Fact]
	public void FilterEntitiesWithNotContains()
	{
		var ids = new List<int> { 1, 2, 3 };
		Expression<Func<TestEntity, bool>> filter = x => !ids.Contains(x.ProductID);
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("not (ProductID in (1,2,3))");
	}

	[Fact]
	public void FilterEntitiesWithContainsAndNotContains()
	{
		var ids = new List<int> { 1, 2, 3 };
		var names = new List<string> { "Chai", "Milk", "Water" };
		Expression<Func<TestEntity, bool>> filter = x => ids.Contains(x.ProductID) && !names.Contains(x.ProductName);
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("(ProductID in (1,2,3)) and not (ProductName in ('Chai','Milk','Water'))");
	}
}

public abstract class TypedExpressionTests : CoreTestBase
{
	[AttributeUsage(AttributeTargets.Property)]
	private class DataAttribute : Attribute
	{
		public string Name { get; set; }
		public string PropertyName { get; set; }
	}

	[AttributeUsage(AttributeTargets.Property)]
	private class DataMemberAttribute : Attribute
	{
		public string Name { get; set; }
		public string PropertyName { get; set; }
	}

	[AttributeUsage(AttributeTargets.Property)]
	private class OtherAttribute : Attribute
	{
		public string Name { get; set; }
		public string PropertyName { get; set; }
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	private class JsonPropertyNameAttribute(string name) : Attribute
	{
		public string Name { get; } = name;
	}

	internal class TestEntity
	{
		public int ProductID { get; set; }

		public string ProductName { get; set; }

		public Guid LinkID { get; set; }

		public decimal Price { get; set; }

		public Address Address { get; set; }

		public DateTime CreationTime { get; set; }

		public DateTimeOffset Updated { get; set; }

		public TimeSpan Period { get; set; }

		public TestEntity Nested { get; set; }

		public TestEntity[] Collection { get; set; }

		[Column(Name = "Name")]
		public string MappedNameUsingColumnAttribute { get; set; }

		[Data(Name = "Name", PropertyName = "OtherName")]
		public string MappedNameUsingDataAttribute { get; set; }

		[DataMember(Name = "Name", PropertyName = "OtherName")]
		public string MappedNameUsingDataMemberAttribute { get; set; }

		[Other(Name = "Name", PropertyName = "OtherName")]
		public string MappedNameUsingOtherAttribute { get; set; }

		[DataMember(Name = "Name", PropertyName = "OtherName")]
		[Other(Name = "OtherName", PropertyName = "OtherName")]
		public string MappedNameUsingDataMemberAndOtherAttribute { get; set; }

		[JsonProperty("Name")]
		public string MappedNameUsingJsonPropertyAttribute { get; set; }

		[JsonPropertyName("Name")]
		public string MappedNameUsingJsonPropertyNameAttribute { get; set; }
	}

	[Fact]
	public void And()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductID == 1 && x.ProductName == "Chai";
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("ProductID eq 1 and ProductName eq 'Chai'");
	}

	[Fact]
	public void Or()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductName == "Chai" || x.ProductID == 1;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("ProductName eq 'Chai' or ProductID eq 1");
	}

	[Fact]
	public void Not()
	{
		Expression<Func<TestEntity, bool>> filter = x => !(x.ProductName == "Chai");
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("not (ProductName eq 'Chai')");
	}

	[Fact]
	public void Precedence()
	{
		Expression<Func<TestEntity, bool>> filter = x => (x.ProductID == 1 || x.ProductID == 2) && x.ProductName == "Chai";
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("(ProductID eq 1 or ProductID eq 2) and ProductName eq 'Chai'");
	}

	[Fact]
	public void EqualString()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductName == "Chai";
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("ProductName eq 'Chai'");
	}

	[Fact]
	public void EqualFieldToString()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductName.ToString() == "Chai";
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("ProductName eq 'Chai'");
	}

	[Fact]
	public void EqualValueToString()
	{
		var name = "Chai";
		Expression<Func<TestEntity, bool>> filter = x => x.ProductName.ToString() == name.ToString();
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("ProductName eq 'Chai'");
	}

	[Fact]
	public void EqualNumeric()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductID == 1;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("ProductID eq 1");
	}

	[Fact]
	public void NotEqualNumeric()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductID != 1;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("ProductID ne 1");
	}

	[Fact]
	public void GreaterNumeric()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductID > 1;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("ProductID gt 1");
	}

	[Fact]
	public void GreaterOrEqualNumeric()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductID >= 1.5;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be($"ProductID ge 1.5{FormatSettings.DoubleNumberSuffix}");
	}

	[Fact]
	public void LessNumeric()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductID < 1;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("ProductID lt 1");
	}

	[Fact]
	public void LessOrEqualNumeric()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductID <= 1;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("ProductID le 1");
	}

	[Fact]
	public void AddEqualNumeric()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductID + 1 == 2;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("ProductID add 1 eq 2");
	}

	[Fact]
	public void SubEqualNumeric()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductID - 1 == 2;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("ProductID sub 1 eq 2");
	}

	[Fact]
	public void MulEqualNumeric()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductID * 1 == 2;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("ProductID mul 1 eq 2");
	}

	[Fact]
	public void DivEqualNumeric()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductID / 1 == 2;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("ProductID div 1 eq 2");
	}

	[Fact]
	public void ModEqualNumeric()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductID % 1 == 2;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("ProductID mod 1 eq 2");
	}

	[Fact]
	public void EqualLong()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductID == 1L;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be($"ProductID eq 1{FormatSettings.LongNumberSuffix}");
	}

	[Fact]
	public void EqualDecimal()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.Price == 1M;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be($"Price eq 1{FormatSettings.DecimalNumberSuffix}");
	}

	[Fact]
	public void EqualDecimalWithFractionalPart()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.Price == 1.23M;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be($"Price eq 1.23{FormatSettings.DecimalNumberSuffix}");
	}

	[Fact]
	public void EqualGuid()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.LinkID == Guid.Empty;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be($"LinkID eq {FormatSettings.GetGuidFormat("00000000-0000-0000-0000-000000000000")}");
	}

	[Fact]
	public void EqualDateTime()
	{
		if (FormatSettings.ODataVersion < 4)
		{
			Expression<Func<TestEntity, bool>> filter = x => x.CreationTime == new DateTime(2013, 1, 1);
			ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("CreationTime eq datetime'2013-01-01T00:00:00'");
		}
	}

	[Fact]
	public void EqualDateTimeOffset()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.Updated == new DateTimeOffset(new DateTime(2013, 1, 1, 0, 0, 0, DateTimeKind.Utc));
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be($"Updated eq {FormatSettings.GetDateTimeOffsetFormat("2013-01-01T00:00:00Z")}");
	}

	[Fact]
	public void EqualTimeSpan()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.Period == new TimeSpan(1, 2, 3);
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be($"Period eq {FormatSettings.TimeSpanPrefix}'PT1H2M3S'");
	}

	[Fact]
	public void LengthOfStringEqual()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductName.Length == 4;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("length(ProductName) eq 4");
	}

	[Fact]
	public void StringToLowerEqual()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductName.ToLower() == "chai";
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("tolower(ProductName) eq 'chai'");
	}

	[Fact]
	public void StringToUpperEqual()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductName.ToUpper() == "CHAI";
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("toupper(ProductName) eq 'CHAI'");
	}

	[Fact]
	public void StringStartsWithEqual()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductName.StartsWith("Ch") == true;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("startswith(ProductName,'Ch') eq true");
	}

	[Fact]
	public void StringEndsWithEqual()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductName.EndsWith("Ch") == true;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("endswith(ProductName,'Ch') eq true");
	}

	[Fact]
	public void StringContainsEqualTrue()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductName.Contains("ai") == true;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be($"{FormatSettings.GetContainsFormat("ProductName", "ai")} eq true");
	}

	[Fact]
	public void StringContainsEqualFalse()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductName.Contains("ai") == false;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be($"{FormatSettings.GetContainsFormat("ProductName", "ai")} eq false");
	}

	[Fact]
	public void StringContains()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductName.Contains("ai");
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be(FormatSettings.GetContainsFormat("ProductName", "ai"));
	}

	[Fact]
	public void StringContainedIn()
	{
		Expression<Func<TestEntity, bool>> filter = x => "Chai".Contains(x.ProductName);
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be(FormatSettings.GetContainedInFormat("ProductName", "Chai"));
	}

	[Fact]
	public void StringNotContains()
	{
		Expression<Func<TestEntity, bool>> filter = x => !x.ProductName.Contains("ai");
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be($"not {FormatSettings.GetContainsFormat("ProductName", "ai")}");
	}

	[Fact]
	public void StringToLowerAndContains()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductName.ToLower().Contains("Chai");
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be(FormatSettings.GetContainsFormat("tolower(ProductName)", "Chai"));
	}

	[Fact]
	public void IndexOfStringEqual()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductName.IndexOf("ai") == 1;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("indexof(ProductName,'ai') eq 1");
	}

	[Fact]
	public void SubstringWithPositionEqual()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductName.Substring(1) == "hai";
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("substring(ProductName,1) eq 'hai'");
	}

	[Fact]
	public void SubstringWithPositionAndLengthEqual()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductName.Substring(1, 2) == "ha";
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("substring(ProductName,1,2) eq 'ha'");
	}

	[Fact]
	public void ReplaceStringEqual()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductName.Replace("a", "o") == "Choi";
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("replace(ProductName,'a','o') eq 'Choi'");
	}

	[Fact]
	public void TrimEqual()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.ProductName.Trim() == "Chai";
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("trim(ProductName) eq 'Chai'");
	}

	[Fact]
	public void ConcatEqual()
	{
		Expression<Func<TestEntity, bool>> filter = x => string.Concat(x.ProductName, "Chai") == "ChaiChai";
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("concat(ProductName,'Chai') eq 'ChaiChai'");
	}

	[Fact]
	public void DayEqual()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.CreationTime.Day == 1;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("day(CreationTime) eq 1");
	}

	[Fact]
	public void MonthEqual()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.CreationTime.Month == 2;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("month(CreationTime) eq 2");
	}

	[Fact]
	public void YearEqual()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.CreationTime.Year == 3;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("year(CreationTime) eq 3");
	}

	[Fact]
	public void HourEqual()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.CreationTime.Hour == 4;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("hour(CreationTime) eq 4");
	}

	[Fact]
	public void MinuteEqual()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.CreationTime.Minute == 5;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("minute(CreationTime) eq 5");
	}

	[Fact]
	public void SecondEqual()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.CreationTime.Second == 6;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("second(CreationTime) eq 6");
	}

	[Fact]
	public void RoundEqual()
	{
		Expression<Func<TestEntity, bool>> filter = x => decimal.Round(x.Price) == 1;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be($"round(Price) eq 1{FormatSettings.DecimalNumberSuffix}");
	}

	[Fact]
	public void FloorEqual()
	{
		Expression<Func<TestEntity, bool>> filter = x => decimal.Floor(x.Price) == 1;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be($"floor(Price) eq 1{FormatSettings.DecimalNumberSuffix}");
	}

	[Fact]
	public void CeilingEqual()
	{
		Expression<Func<TestEntity, bool>> filter = x => decimal.Ceiling(x.Price) == 2;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be($"ceiling(Price) eq 2{FormatSettings.DecimalNumberSuffix}");
	}

	[Fact]
	public void EqualNestedProperty()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.Nested.ProductID == 1;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("Nested/ProductID eq 1");
	}

	[Fact]
	public void EqualNestedPropertyLengthOfStringEqual()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.Nested.ProductName.Length == 4;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("length(Nested/ProductName) eq 4");
	}

	[Fact]
	public void ConvertEqual()
	{
		var id = "1";
		Expression<Func<TestEntity, bool>> filter = x => x.Nested.ProductID == Convert.ToInt32(id);
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("Nested/ProductID eq 1");
	}

	[Fact]
	public void FilterWithMappedPropertiesUsingColumnAttribute()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.MappedNameUsingColumnAttribute == "Milk";
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("Name eq 'Milk'");
	}

	[Fact]
	public void FilterWithMappedPropertiesUsingDataAttribute()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.MappedNameUsingDataAttribute == "Milk";
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("Name eq 'Milk'");
	}

	[Fact]
	public void FilterWithMappedPropertiesUsingDataMemberAttribute()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.MappedNameUsingDataMemberAttribute == "Milk";
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("Name eq 'Milk'");
	}

	[Fact]
	public void FilterWithMappedPropertiesUsingOtherAttribute()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.MappedNameUsingOtherAttribute == "Milk";
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("Name eq 'Milk'");
	}

	[Fact]
	public void FilterWithMappedPropertiesUsingDataMemberAndOtherAttribute()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.MappedNameUsingDataMemberAndOtherAttribute == "Milk";
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("Name eq 'Milk'");
	}

	[Fact]
	public void FilterWithMappedPropertiesUsingJsonPropertyAttribute()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.MappedNameUsingJsonPropertyAttribute == "Milk";
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("Name eq 'Milk'");
	}

	[Fact]
	public void FilterWithMappedPropertiesUsingJsonPropertyNameAttribute()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.MappedNameUsingJsonPropertyNameAttribute == "Milk";
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("Name eq 'Milk'");
	}

	[Fact]
	public void FilterWithEnum()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.Address.Type == AddressType.Corporate;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be($"Address/Type eq {FormatSettings.GetEnumFormat(AddressType.Corporate, typeof(AddressType), "NorthwindModel")}");
	}

	[Fact]
	public void FilterWithEnum_LocalVar()
	{
		var addressType = AddressType.Corporate;
		Expression<Func<TestEntity, bool>> filter = x => x.Address.Type == addressType;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be($"Address/Type eq {FormatSettings.GetEnumFormat(AddressType.Corporate, typeof(AddressType), "NorthwindModel")}");
	}

	private readonly AddressType addressType = AddressType.Corporate;

	[Fact]
	public void FilterWithEnum_MemberVar()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.Address.Type == addressType;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be($"Address/Type eq {FormatSettings.GetEnumFormat(AddressType.Corporate, typeof(AddressType), "NorthwindModel")}");
	}

	[Fact]
	public void FilterWithEnum_Const()
	{
		const AddressType addressType = AddressType.Corporate;
		Expression<Func<TestEntity, bool>> filter = x => x.Address.Type == addressType;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be($"Address/Type eq {FormatSettings.GetEnumFormat(AddressType.Corporate, typeof(AddressType), "NorthwindModel")}");
	}

	[Fact]
	public void FilterWithEnum_PrefixFree()
	{
		var enumPrefixFree = _session.Settings.EnumPrefixFree;
		_session.Settings.EnumPrefixFree = true;
		try
		{
			Expression<Func<TestEntity, bool>> filter = x => x.Address.Type == AddressType.Corporate;
			ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be($"Address/Type eq {FormatSettings.GetEnumFormat(AddressType.Corporate, typeof(AddressType), "NorthwindModel", true)}");
		}
		finally
		{
			_session.Settings.EnumPrefixFree = enumPrefixFree;
		}
	}

	[Fact]
	public void FilterWithEnum_HasFlag()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.Address.Type.HasFlag(AddressType.Corporate);
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be($"Address/Type has {FormatSettings.GetEnumFormat(AddressType.Corporate, typeof(AddressType), "NorthwindModel")}");
	}

	[Fact]
	public void FilterWithEnum_ToString()
	{
		Expression<Func<TestEntity, bool>> filter = x => x.Address.Type.ToString() == AddressType.Corporate.ToString();
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be($"Address/Type eq {FormatSettings.GetEnumFormat(AddressType.Corporate, typeof(AddressType), "NorthwindModel")}");
	}

	[Fact]
	public void FilterDateTimeRange()
	{
		var beforeDT = new DateTime(2013, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		var afterDT = new DateTime(2014, 2, 2, 0, 0, 0, DateTimeKind.Utc);
		Expression<Func<TestEntity, bool>> filter = x => (x.CreationTime >= beforeDT) && (x.CreationTime < afterDT);
		if (FormatSettings.ODataVersion < 4)
		{
			ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("CreationTime ge datetime'2013-01-01T00:00:00Z' and CreationTime lt datetime'2014-02-02T00:00:00Z'");
		}
		else
		{
			ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("CreationTime ge 2013-01-01T00:00:00Z and CreationTime lt 2014-02-02T00:00:00Z");
		}
	}

	[Fact]
	public void ExpressionBuilder()
	{
		Expression<Predicate<TestEntity>> condition1 = x => x.ProductName == "Chai";
		Expression<Func<TestEntity, bool>> condition2 = x => x.ProductID == 1;
		var filter = new ODataExpression(condition1);
		filter = filter || new ODataExpression(condition2);
		filter.AsString(_session).Should().Be("ProductName eq 'Chai' or ProductID eq 1");
	}

	[Fact]
	public void ExpressionBuilderGeneric()
	{
		var filter = new ODataExpression<TestEntity>(x => x.ProductName == "Chai");
		filter = filter || new ODataExpression<TestEntity>(x => x.ProductID == 1);
		filter.AsString(_session).Should().Be("ProductName eq 'Chai' or ProductID eq 1");
	}

	[Fact]
	public void ExpressionBuilderGrouping()
	{
		Expression<Predicate<TestEntity>> condition1 = x => x.ProductName == "Chai";
		Expression<Func<TestEntity, bool>> condition2 = x => x.ProductID == 1;
		Expression<Predicate<TestEntity>> condition3 = x => x.ProductName == "Kaffe";
		Expression<Func<TestEntity, bool>> condition4 = x => x.ProductID == 2;
		var filter1 = new ODataExpression(condition1) || new ODataExpression(condition2);
		var filter2 = new ODataExpression(condition3) || new ODataExpression(condition4);
		var filter = filter1 && filter2;
		filter.AsString(_session).Should().Be("(ProductName eq 'Chai' or ProductID eq 1) and (ProductName eq 'Kaffe' or ProductID eq 2)");
	}

	[Fact]
	public void FilterEqualityToMappedPropertyOfOtherEntity()
	{
		var otherEntity = new TestEntity
		{
			MappedNameUsingDataMemberAttribute = "Other Name"
		};
		Expression<Func<TestEntity, bool>> filter = x => x.MappedNameUsingDataMemberAttribute == otherEntity.MappedNameUsingDataMemberAttribute;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("Name eq 'Other Name'");

		otherEntity = new TestEntity
		{
			MappedNameUsingJsonPropertyAttribute = "Other Name"
		};
		filter = x => x.MappedNameUsingJsonPropertyAttribute == otherEntity.MappedNameUsingJsonPropertyAttribute;
		ODataExpression.FromLinqExpression(filter).AsString(_session).Should().Be("Name eq 'Other Name'");
	}
}

