namespace Simple.OData.Client.Extensions;


internal static class EnumerableOfKeyValuePairExtensions
{

#if NETSTANDARD2_0

	//public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
	//{
	//	if (source is not Dictionary<TKey, TValue> dictionary)
	//	{
	//		dictionary = source.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
	//	}

	//	return dictionary;
	//}

#endif

	public static IDictionary<TKey, TValue> ToIDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
	{
		if (source is not Dictionary<TKey, TValue> dictionary)
		{
			dictionary = source.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
		}

		return dictionary;
	}

}

