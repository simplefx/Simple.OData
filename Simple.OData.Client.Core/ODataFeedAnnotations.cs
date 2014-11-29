using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.OData.Client
{
    public class ODataFeedAnnotations
    {
        public string Id { get; internal set; }
        public long? Count { get; internal set; }
        public Uri DeltaLink { get; internal set; }
        public Uri NextPageLink { get; internal set; }
        public IEnumerable<object> InstanceAnnotations { get; internal set; }

        public IEnumerable<T> GetInstanceAnnotations<T>()
        {
            return this.InstanceAnnotations.Select(x => (T)x);
        }

        public void CopyFrom(ODataFeedAnnotations src)
        {
            this.Id = src.Id;
            this.Count = src.Count;
            this.DeltaLink = src.DeltaLink;
            this.NextPageLink = src.NextPageLink;
            this.InstanceAnnotations = src.InstanceAnnotations;
        }
    }
}