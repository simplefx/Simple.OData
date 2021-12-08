namespace Simple.OData.Client
{
	public class ResponseNode
	{
		public AnnotatedFeed Feed { get; set; }
		public AnnotatedEntry Entry { get; set; }
		public string LinkName { get; set; }

		public object Value => Feed != null && Feed.Entries != null
					? (object)Feed.Entries
					: Entry != null
					? Entry.Data
					: null;
	}
}