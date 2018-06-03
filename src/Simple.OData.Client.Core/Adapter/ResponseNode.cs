using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public class ResponseNode
    {
        public AnnotatedFeed Feed { get; set; }
        public AnnotatedEntry Entry { get; set; }
        public string LinkName { get; set; }

        public object Value
        {
            get
            {
                return
                    this.Feed != null && this.Feed.Entries !=null && this.Feed.Entries.Any()
                    ? (object)this.Feed.Entries.Select(x => x.Data)
                    : this.Entry != null
                    ? this.Entry.Data
                    : null;
            }
        }
    }
}