using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Simple.OData.Client
{
    class MetadataCache
    {
        public static readonly SimpleDictionary<string, MetadataCache> Instances = new SimpleDictionary<string, MetadataCache>();

        private string _metadataString;

        public bool IsResolved()
        {
            return _metadataString != null;
        }

        public string MetadataAsString
        {
            get
            {
                if (_metadataString == null)
                    throw new InvalidOperationException("Service metadata is not resolved");

                return _metadataString;
            }
        }

        public void SetMetadataString(string metadataString)
        {
            _metadataString = metadataString;
        }
    }
}
