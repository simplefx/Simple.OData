using System.Collections.Generic;
using Microsoft.OData;

namespace Simple.OData.Client.V4.Adapter
{
    public class ODataResourceEx
    {
        public string TypeName { get; set; }
        public IEnumerable<ODataProperty> PrimitiveProperties { get; set; }
        public IDictionary<string, ODataResourceEx> StructuralProperties { get; set; }

        public ODataResource GetPrimitiveResource()
        {
            return new ODataResource {TypeName = this.TypeName, Properties = this.PrimitiveProperties};
        }
    }
}