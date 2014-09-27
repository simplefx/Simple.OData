using System.Collections.Generic;

namespace Simple.OData.Client
{
    public class ReferenceLink
    {
        public string LinkName { get; set; }
        public object LinkData { get; set; }
        public string ContentId { get; set; }
    }

    public class EntryDetails
    {
        private readonly IDictionary<string, object> _properties = new Dictionary<string, object>();
        private readonly List<ReferenceLink> _links = new List<ReferenceLink>();

        public IDictionary<string, object> Properties { get { return _properties; } }
        public IEnumerable<ReferenceLink> Links { get { return _links; } }

        public void AddProperty(string propertyName, object propertyValue)
        {
            _properties.Add(propertyName, propertyValue);
        }

        public void AddLink(string linkName, object linkData, string contentId = null)
        {
            _links.Add(new ReferenceLink() {LinkName = linkName, LinkData = linkData, ContentId = contentId});
        }
    }
}
