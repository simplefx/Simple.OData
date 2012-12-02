using System.Dynamic;

namespace Simple.OData.Client
{
    public static class ODataFilter
    {
        public class DynamicContext : DynamicObject
        {
            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                result = FilterExpression.FromReference(binder.Name);
                return true;
            }
        }

        public static dynamic Context
        {
            get { return new DynamicContext(); }
        }
    }
}
