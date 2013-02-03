using System.Collections.Generic;

namespace Simple.OData.Client
{
    internal class EntryMembers
    {
        private readonly IDictionary<string, object> _properties = new Dictionary<string, object>();
        private readonly List<KeyValuePair<string, object>> _associationsByValue = new List<KeyValuePair<string, object>>();
        private readonly List<KeyValuePair<string, int>> _associationsByContentId = new List<KeyValuePair<string, int>>();

        public IDictionary<string, object> Properties { get { return _properties; } }
        public List<KeyValuePair<string, object>> AssociationsByValue { get { return _associationsByValue; } }
        public List<KeyValuePair<string, int>> AssociationsByContentId { get { return _associationsByContentId; } }

        public void AddProperty(string propertyName, object propertyValue)
        {
            _properties.Add(propertyName, propertyValue);
        }

        public void AddAssociationByValue(string associationName, object associatedData)
        {
            _associationsByValue.Add(new KeyValuePair<string, object>(associationName, associatedData));
        }

        public void AddAssociationByContentId(string associationName, int contentId)
        {
            _associationsByContentId.Add(new KeyValuePair<string, int>(associationName, contentId));
        }
    }
}
