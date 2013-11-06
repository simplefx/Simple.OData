using System.Collections.Generic;

namespace Simple.OData.Client
{
    public static class ODataDynamic
    {
        public static dynamic Expression
        {
            get { return new DynamicODataExpression(); }
        }

        public static ODataExpression ExpressionFromReference(string reference)
        {
            return DynamicODataExpression.FromReference(reference);
        }

        public static ODataExpression ExpressionFromValue(object value)
        {
            return DynamicODataExpression.FromValue(value);
        }

        public static ODataExpression ExpressionFromFunction(string functionName, string targetName, IEnumerable<object> arguments)
        {
            return DynamicODataExpression.FromFunction(functionName, targetName, arguments);
        }
    }
}