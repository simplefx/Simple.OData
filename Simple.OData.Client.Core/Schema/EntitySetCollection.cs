using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Simple.OData.Client.Extensions;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public class EntitySetCollection : Collection<EntitySet>
    {
        public EntitySetCollection()
        {
        }

        public EntitySetCollection(IEnumerable<EntitySet> entitySets)
            : base(entitySets.ToList())
        {
        }

        public EntitySet Find(string entitySetName)
        {
            var entitySet = TryFind(entitySetName)
                   ?? TryFind(entitySetName.Singularize())
                   ?? TryFind(entitySetName.Pluralize());

            if (entitySet == null)
                throw new UnresolvableObjectException(entitySetName, string.Format("EntitySet {0} not found", entitySetName));

            return entitySet;
        }

        public bool Contains(string entitySetName)
        {
            return TryFind(entitySetName) != null;
        }

        private EntitySet TryFind(string entitySetName)
        {
            return this.SingleOrDefault(t => t.ActualName.Homogenize().Equals(entitySetName.Homogenize()));
        }
    }
}
