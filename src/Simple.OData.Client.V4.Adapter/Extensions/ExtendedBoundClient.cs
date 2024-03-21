namespace Simple.OData.Client.V4.Adapter.Extensions
{
	/// <summary>
	/// Provides access to extended OData operations e.g. data aggregation extensions in a fluent style.
	/// </summary>
	/// <typeparam name="T">The entry type.</typeparam>
	/// <inheritdoc cref="IExtendedBoundClient{T}"/>
	public class ExtendedBoundClient<T> : BoundClient<T>, IExtendedBoundClient<T> where T : class
	{
		private ExtendedBoundClient(Session session, FluentCommand command)
			: base(new ODataClient(session.Settings), session, null, command)
		{
		}

		internal ExtendedBoundClient(ODataClient oDataClient, Session session, bool dynamicResults = false)
			: base(oDataClient, session, dynamicResults: dynamicResults)
		{
		}

		public IExtendedBoundClient<TR> Apply<TR>(Func<IDataAggregation<T>, IDataAggregation<TR>> dataAggregation) where TR : class
		{
			var dataAggregationBuilder = new DataAggregationBuilder<T>(Session);
			dataAggregation(dataAggregationBuilder);
			AppendDataAggregationBuilder(dataAggregationBuilder);
			return new ExtendedBoundClient<TR>(Session, Command);
		}

		public IExtendedBoundClient<T> Apply(string dataAggregationCommand)
		{
			AppendDataAggregationCommand(dataAggregationCommand);
			return this;
		}

		public IExtendedBoundClient<TR> Apply<TR>(string dataAggregationCommand) where TR : class
		{
			AppendDataAggregationCommand(dataAggregationCommand);
			return new ExtendedBoundClient<TR>(Session, Command);
		}

		public IExtendedBoundClient<T> Apply(DynamicDataAggregation dataAggregation)
		{
			Command.Details.Extensions[ODataLiteral.Apply] = dataAggregation.CreateBuilder();
			return this;
		}

		private void AppendDataAggregationBuilder(DataAggregationBuilder dataAggregationBuilder)
		{
			if (Command.Details.Extensions.TryGetValue(ODataLiteral.Apply, out var applyExtension) &&
				applyExtension is DataAggregationBuilder actualDataAggregationBuilder)
			{
				actualDataAggregationBuilder.Append(dataAggregationBuilder);
			}
			else
			{
				Command.Details.Extensions[ODataLiteral.Apply] = dataAggregationBuilder;
			}
		}

		private void AppendDataAggregationCommand(string dataAggregationCommand)
		{
			Command.Details.Extensions[ODataLiteral.Apply] = Command.Details.Extensions.TryGetValue(ODataLiteral.Apply, out var applyExtension) &&
				applyExtension is string actualDataAggregationCommand && !string.IsNullOrEmpty(actualDataAggregationCommand)
				? actualDataAggregationCommand + "/" + dataAggregationCommand
				: dataAggregationCommand;
		}
	}
}