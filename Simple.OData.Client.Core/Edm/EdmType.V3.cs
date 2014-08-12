using Microsoft.Data.Edm;

namespace Simple.OData.Client
{
    public sealed partial class EdmType
    {
        public static EdmType FromModel(IEdmTypeReference type)
        {
            return new EdmType(type.FullName());
        }
    }
}