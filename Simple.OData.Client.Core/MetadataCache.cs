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
        private readonly Func<ODataProvider> _providerFunc;
        private readonly Lazy<Collection<EntitySet>> _lazyEntitySets;

        public MetadataCache(Func<ODataProvider> providerFunc)
        {
            _providerFunc = providerFunc;
            _lazyEntitySets = new Lazy<Collection<EntitySet>>(CreateEntitySetCollection);
        }

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

        private IMetadata Metadata
        {
            get { return _providerFunc().GetMetadata(); }
        }

        private IEnumerable<EntitySet> EntitySets
        {
            get { return _lazyEntitySets.Value.AsEnumerable(); }
        }

        private Collection<EntitySet> CreateEntitySetCollection()
        {
            return new Collection<EntitySet>(this.Metadata.GetEntitySetNames().Select(x => new EntitySet(x, null, this.Metadata)).ToList());
        }

        public EntitySet FindEntitySet(string entitySetName)
        {
            var actualName = this.Metadata.GetEntitySetExactName(entitySetName);
            return this.EntitySets.Single(x => x.ActualName == actualName);
        }

        public EntitySet FindBaseEntitySet(string entitySetPath)
        {
            return this.FindEntitySet(entitySetPath.Split('/').First());
        }

        public EntitySet FindConcreteEntitySet(string entitySetPath)
        {
            var items = entitySetPath.Split('/');
            if (items.Count() > 1)
            {
                var baseEntitySet = this.FindEntitySet(items[0]);
                var entitySet = string.IsNullOrEmpty(items[1])
                    ? baseEntitySet
                    : baseEntitySet.FindDerivedEntitySet(items[1]);
                return entitySet;
            }
            else
            {
                return this.FindEntitySet(entitySetPath);
            }
        }
    }
}
