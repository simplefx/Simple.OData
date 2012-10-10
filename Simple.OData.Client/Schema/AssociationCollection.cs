using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Simple.OData.Client
{
    public class AssociationCollection : Collection<Association>
    {
        public AssociationCollection()
        {
        }

        public AssociationCollection(IEnumerable<Association> associations)
            : base(associations.ToList())
        {
        }

        public Association Find(string associationName)
        {
            var association = TryFind(associationName);
            if (association == null) throw new UnresolvableObjectException(associationName, string.Format("Association {0} not found", associationName));
            return association;
        }

        public bool Contains(string associationName)
        {
            return TryFind(associationName) != null;
        }

        private Association TryFind(string associationName)
        {
            associationName = associationName.Homogenize(); 
            return this
                .Where(c => c.HomogenizedActualName.Equals(associationName))
                .SingleOrDefault();
        }
    }
}
