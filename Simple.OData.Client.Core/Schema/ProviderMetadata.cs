using System.Collections.Generic;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    abstract class ProviderMetadata
    {
        public string ProtocolVersion { get; set; }
        public object Model { get; set; }

        public abstract IEnumerable<string> GetStructuralPropertiesNames(string entitySetName);
        public abstract bool HasStructuralProperty(string entitySetName, string propertyName);
        public abstract string GetStructuralPropertyExactName(string entitySetName, string propertyName);
        public abstract EdmPropertyType GetStructuralPropertyType(string entitySetName, string propertyName);

        public abstract bool HasNavigationProperty(string entitySetName, string propertyName);
        public abstract string GetNavigationPropertyExactName(string entitySetName, string propertyName);
        public abstract string GetNavigationPropertyPartnerName(string entitySetName, string propertyName);
        public abstract bool IsNavigationPropertyMultiple(string entitySetName, string propertyName);

        public abstract string GetFunctionActualName(string functionName);

        protected bool NamesAreEqual(string actualName, string requestedName)
        {
            return actualName.Homogenize() == requestedName.Homogenize()
                   || actualName.Homogenize() == requestedName.Singularize().Homogenize()
                   || actualName.Homogenize() == requestedName.Pluralize().Homogenize();
        }
    }
}