namespace Simple.OData.Client
{
	public class ResponseNode
	{
		public AnnotatedFeed Feed { get; set; }
		public AnnotatedEntry Entry { get; set; }
		public string LinkName { get; set; }

		public object Value => this.Feed != null && this.Feed.Entries != null
					? (object)this.Feed.Entries
					: this.Entry != null
					? this.Entry.Data
					: null;
	}
}