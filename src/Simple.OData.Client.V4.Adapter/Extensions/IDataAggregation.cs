using System.Linq.Expressions;

namespace Simple.OData.Client.V4.Adapter.Extensions;

public interface IDataAggregation<T> where T : class
{
	/// <summary>
	/// Sets the specified OData filter.
	/// </summary>
	/// <param name="filter">The filter expression.</param>
	/// <returns>Self.</returns>
	IDataAggregation<T> Filter(Expression<Func<T, bool>> filter);

	/// <summary>
	/// Sets the specified aggregation
	/// </summary>
	/// <param name="aggregation">Aggregation expression, should be a NewExpression or MemberInitExpression e.g. (x, a) => new {Total = a.Sum(x.Count)}</param>
	/// <typeparam name="TR">Destination type</typeparam>
	/// <returns>New data aggregation builder of destination type</returns>
	IDataAggregation<TR> Aggregate<TR>(Expression<Func<T, IAggregationFunction<T>, TR>> aggregation) where TR : class;

	/// <summary>
	/// Sets the specified group by expression and aggregation
	/// </summary>
	/// <param name="groupBy">Group by expression, should be a NewExpression or MemberInitExpression e.g. (x, a) => new {CategoryName = x.CategoryName, AveragePrice = a.Sum(x.Price)}</param>
	/// <typeparam name="TR">Destination type</typeparam>
	/// <returns>New data aggregation builder of destination type</returns>
	IDataAggregation<TR> GroupBy<TR>(Expression<Func<T, IAggregationFunction<T>, TR>> groupBy) where TR : class;
}
