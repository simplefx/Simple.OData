using System.Collections.Generic;
using System.Dynamic;

namespace Simple.OData.Client
{
    public static class ODataFilter
    {
        public class ExpressionFactory : DynamicObject
        {
            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                result = FilterExpression.FromReference(binder.Name);
                return true;
            }
        }

        public static dynamic Expression
        {
            get { return new ExpressionFactory(); }
        }

        public static FilterExpression ExpressionFromReference(string reference)
        {
            return FilterExpression.FromReference(reference);
        }

        public static FilterExpression ExpressionFromValue(object value)
        {
            return FilterExpression.FromValue(value);
        }

        public static FilterExpression ExpressionFromFunction(string functionName, string targetName, IEnumerable<object> arguments)
        {
            return FilterExpression.FromFunction(functionName, targetName, arguments);
        }
    }
}
