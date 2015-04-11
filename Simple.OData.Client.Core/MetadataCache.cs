using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.OData.Client
{
    class MetadataCache
    {
        public static readonly SimpleDictionary<string, MetadataCache> Instances = new SimpleDictionary<string, MetadataCache>();

        private string _metadataDocument;

        public bool IsResolved()
        {
            return _metadataDocument != null;
        }

        public string MetadataDocument
        {
            get
            {
                if (_metadataDocument == null)
                    throw new InvalidOperationException("Service metadata is not resolved");

                return _metadataDocument;
            }
        }

        public void SetMetadataDocument(string metadataString)
        {
            _metadataDocument = metadataString;
        }
    }
}
