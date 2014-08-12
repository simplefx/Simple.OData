using Microsoft.OData.Edm;

namespace Simple.OData.Client
{
    public sealed partial class EdmType
    {
        public static EdmType FromODataType(IEdmTypeReference type)
        {
            return new EdmType(type.FullName());
        }
    }
}