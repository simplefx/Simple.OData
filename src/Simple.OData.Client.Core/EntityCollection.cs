namespace Simple.OData.Client
{
	public class EntityCollection
	{
		private readonly EntityCollection _baseEntityCollection;

		internal EntityCollection(string name, EntityCollection baseEntityCollection = null)
		{
			Name = name;
			_baseEntityCollection = baseEntityCollection;
		}

		public override string ToString()
		{
			return Name;
		}

		public string Name { get; private set; }

		public EntityCollection BaseEntityCollection => _baseEntityCollection;
	}
}
