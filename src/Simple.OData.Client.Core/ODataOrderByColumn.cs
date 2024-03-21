namespace Simple.OData.Client;

public class ODataOrderByColumn : IEquatable<ODataOrderByColumn>
{
	public string Name { get; }
	public bool Descending { get; }

	public ODataOrderByColumn(string name, bool descending)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentException($"Parameter {nameof(name)} should not be null or empty.", nameof(name));
		}

		Name = name;
		Descending = descending;
	}

	public bool Equals(ODataOrderByColumn other)
	{
		if (other is null)
		{
			return false;
		}

		if (ReferenceEquals(this, other))
		{
			return true;
		}

		return Name == other.Name && Descending == other.Descending;
	}

	public override bool Equals(object obj)
	{
		if (obj is null)
		{
			return false;
		}

		if (ReferenceEquals(this, obj))
		{
			return true;
		}

		if (obj.GetType() != GetType())
		{
			return false;
		}

		return Equals((ODataOrderByColumn)obj);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			return ((Name is not null ? Name.GetHashCode() : 0) * 397) ^ Descending.GetHashCode();
		}
	}
}
