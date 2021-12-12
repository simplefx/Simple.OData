namespace Simple.OData.Client.V4.Adapter.Extensions;

public sealed class DynamicAggregationFunction
{
	internal DynamicAggregationFunction()
	{
	}

	public (string, ODataExpression) Average(ODataExpression expression)
	{
		return (nameof(Average), expression);
	}

	public (string, ODataExpression) Sum(ODataExpression expression)
	{
		return (nameof(Sum), expression);
	}

	public (string, ODataExpression) Min(ODataExpression expression)
	{
		return (nameof(Min), expression);
	}

	public (string, ODataExpression) Max(ODataExpression expression)
	{
		return (nameof(Max), expression);
	}

	public (string, ODataExpression) Count()
	{
		return (nameof(Count), null);
	}

	public (string, ODataExpression) CountDistinct(ODataExpression expression)
	{
		return (nameof(CountDistinct), expression);
	}
}
