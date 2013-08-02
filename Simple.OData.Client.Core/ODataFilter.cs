using System.Collections.Generic;
using System.Dynamic;

#if !PORTABLE_IOS
namespace Simple.OData.Client
{
    public static class ODataFilter
    {
        public static dynamic Expression
        {
            get { return new DynamicFilterExpression(); }
        }

        public static FilterExpression ExpressionFromReference(string reference)
        {
            return DynamicFilterExpression.FromReference(reference);
        }

        public static FilterExpression ExpressionFromValue(object value)
        {
            return DynamicFilterExpression.FromValue(value);
        }

        public static FilterExpression ExpressionFromFunction(string functionName, string targetName, IEnumerable<object> arguments)
        {
            return DynamicFilterExpression.FromFunction(functionName, targetName, arguments);
        }
    }
}
#endif
