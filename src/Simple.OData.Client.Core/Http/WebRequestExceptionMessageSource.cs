namespace Simple.OData.Client
{
	/// <summary>
	/// The message of an exception raised after a non successful Http request.
	/// </summary>
	public enum WebRequestExceptionMessageSource
	{
		/// <summary>
		/// The reason phrase of the HTTP response message.
		/// </summary>
		ReasonPhrase,

		/// <summary>
		/// The content of the HTTP response message.
		/// </summary>
		ResponseContent,

		/// <summary>
		/// Both the reason phrase and content of the http response message.
		/// </summary>
		Both
	}
}
