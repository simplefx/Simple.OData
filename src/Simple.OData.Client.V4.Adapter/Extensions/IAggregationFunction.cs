namespace Simple.OData.Client.V4.Adapter.Extensions
{
    public interface IAggregationFunction<T>
    {
        TR Average<TR>(TR property);

        TR Sum<TR>(TR property);

        TR Min<TR>(TR property);

        TR Max<TR>(TR property);

        int Count();

        int CountDistinct<TR>(TR property);
    }
}