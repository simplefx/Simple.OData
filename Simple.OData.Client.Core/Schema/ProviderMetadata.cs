using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    abstract class ProviderMetadata
    {
        public string ProtocolVersion { get; set; }
        public object Model { get; set; }

        public abstract IEnumerable<string> GetEntitySetNames();
        public abstract string GetEntitySetExactName(string entitySetName);
        public abstract string GetEntitySetTypeName(string entitySetName);
        public abstract string GetEntitySetTypeNamespace(string entitySetName);
        public abstract string GetDerivedEntityTypeExactName(string entitySetName, string entityTypeName);
        public abstract IEnumerable<string> GetDerivedEntityTypeNames(string entitySetName);
        public abstract bool EntitySetTypeRequiresOptimisticConcurrencyCheck(string entitySetName);

        public abstract string GetEntityTypeExactName(string entityTypeName);

        public abstract IEnumerable<string> GetStructuralPropertyNames(string entitySetName);
        public abstract bool HasStructuralProperty(string entitySetName, string propertyName);
        public abstract string GetStructuralPropertyExactName(string entitySetName, string propertyName);
        public abstract EdmPropertyType GetStructuralPropertyType(string entitySetName, string propertyName);
        public abstract IEnumerable<string> GetDeclaredKeyPropertyNames(string entitySetName);

        public abstract bool HasNavigationProperty(string entitySetName, string propertyName);
        public abstract string GetNavigationPropertyExactName(string entitySetName, string propertyName);
        public abstract string GetNavigationPropertyPartnerName(string entitySetName, string propertyName);
        public abstract bool IsNavigationPropertyMultiple(string entitySetName, string propertyName);

        public abstract string GetFunctionExactName(string functionName);

        public abstract string CreateEntry(string entityTypeNamespace, string entityTypeName, IDictionary<string, object> row);
        public abstract Func<HttpResponseMessage, IProviderResponseReader> GetResponseReaderFunc(bool includeResourceTypeInEntryProperties);

        public static bool NamesAreEqual(string actualName, string requestedName)
        {
            return actualName.Homogenize() == requestedName.Homogenize()
                   || actualName.Homogenize() == requestedName.Singularize().Homogenize()
                   || actualName.Homogenize() == requestedName.Pluralize().Homogenize();
        }

        public static string StreamToString(Stream stream)
        {
            stream.Position = 0;
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}