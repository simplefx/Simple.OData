using System;

namespace Simple.OData.Client.V4.Adapter.Extensions
{
    /// <summary>
    /// Provides access to extended collection-bound OData operations e.g. data aggregation extensions in a fluent style.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public interface IExtendedBoundClient<T> : IBoundClient<T> where T : class
    {
        /// <summary>
        /// Applies the configured data aggregation to the entries
        /// </summary>
        /// <param name="dataAggregation">Data aggregation builder</param>
        /// <typeparam name="T">Source entry type</typeparam>
        /// <typeparam name="TR">Destination entry type</typeparam>
        /// <returns>New bound client with applied data aggregation</returns>
        IExtendedBoundClient<TR> Apply<TR>(Func<IDataAggregation<T>, IDataAggregation<TR>> dataAggregation) where TR : class;
        
        /// <summary>
        /// Applies the specified data aggregation command to the entries
        /// </summary>
        /// <param name="dataAggregationCommand">Data aggregation command</param>
        /// <typeparam name="T">Entry type</typeparam>
        /// <returns>Self.</returns>
        IExtendedBoundClient<T> Apply(string dataAggregationCommand);
        
        /// <summary>
        /// Applies the specified data aggregation command to the entries
        /// </summary>
        /// <param name="dataAggregationCommand">Data aggregation command</param>
        /// <typeparam name="T">Source entry type</typeparam>
        /// <typeparam name="TR">Destination entry type</typeparam>
        /// <returns>New bound client with applied data aggregation</returns>
        IExtendedBoundClient<TR> Apply<TR>(string dataAggregationCommand) where TR : class;
        
        /// <summary>
        /// Applies the specified data aggregation builder to the entries
        /// </summary>
        /// <param name="dataAggregation">Dynamic data aggregation builder</param>
        /// <typeparam name="T">Source entry type</typeparam>
        /// <returns>New bound client with applied data aggregation</returns>
        IExtendedBoundClient<T> Apply(DynamicDataAggregation dataAggregation);
    }
}