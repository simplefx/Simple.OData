using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public class ResponseNode
    {
        public class AnnotatedFeed
        {
            public IList<AnnotatedEntry> Data { get; set; }
            public ODataFeedAnnotations Annotations { get; set; }

            public AnnotatedFeed() { this.Data = new List<AnnotatedEntry>(); }
        }

        public class AnnotatedEntry
        {
            public IDictionary<string, object> Data { get; set; }
            public ODataEntryAnnotations Annotations { get; set; }

            public AnnotatedEntry() { this.Data = new Dictionary<string, object>(); }
        }

        public AnnotatedFeed Feed { get; set; }
        public AnnotatedEntry Entry { get; set; }
        public string LinkName { get; set; }

        public object Value
        {
            get
            {
                return
                    this.Feed != null && this.Feed.Data !=null && this.Feed.Data.Any()
                    ? (object)this.Feed.Data.Select(x => x.Data)
                    : this.Entry != null
                    ? this.Entry.Data
                    : null;
            }
        }
    }
}