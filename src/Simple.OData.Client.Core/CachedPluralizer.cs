using System.Collections.Concurrent;

namespace Simple.OData.Client;

public class CachedPluralizer(IPluralizer pluralizer) : IPluralizer
{
	private readonly IPluralizer pluralizer = pluralizer;
	private readonly ConcurrentDictionary<string, string> singles = new ConcurrentDictionary<string, string>();
	private readonly ConcurrentDictionary<string, string> plurals = new ConcurrentDictionary<string, string>();

	public string Pluralize(string word)
	{
		return plurals.GetOrAdd(word, x => pluralizer.Pluralize(x));
	}

	public string Singularize(string word)
	{
		return singles.GetOrAdd(word, x => pluralizer.Singularize(x));
	}
}
