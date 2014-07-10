using System;
using System.Collections.Generic;
using System.Dynamic;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    [Obsolete("Use ODataDynamic.Expression instead of ODataFilter.Expression", false)]
    public static class ODataFilter
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
