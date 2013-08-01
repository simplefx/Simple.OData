using System.Collections.Generic;
using System.Dynamic;

#if !PORTABLE_IOS
namespace Simple.OData.Client
{
    public static class ODataFilter
    {
        public static dynamic Expression
        {
            get { return new FilterExpression(); }
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
#endif
