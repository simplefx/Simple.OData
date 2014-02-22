using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Simple.OData.Client
{
    public partial class ODataExpression
    {
        private readonly ODataExpression _functionCaller;
        private readonly ODataExpression _left;
        private readonly ODataExpression _right;
        private readonly ExpressionOperator _operator;

        public string Reference { get; private set; }
        public object Value { get; private set; }
        public ExpressionFunction Function { get; private set; }

        internal ODataExpression()
        {
        }

        protected ODataExpression(object value)
        {
            this.Value = value;
        }

        protected ODataExpression(string reference)
        {
            this.Reference = reference;
        }

        protected ODataExpression(string reference, object value)
        {
            this.Reference = reference;
            this.Value = value;
        }

        protected ODataExpression(ExpressionFunction function)
        {
            this.Function = function;
        }

        protected ODataExpression(ODataExpression left, ODataExpression right, ExpressionOperator expressionOperator)
        {
            _left = left;
            _right = right;
            _operator = expressionOperator;
        }

        protected ODataExpression(ODataExpression caller, string reference)
        {
            _functionCaller = caller;
            this.Reference = reference;
        }

        protected ODataExpression(ODataExpression caller, ExpressionFunction function)
        {
            _functionCaller = caller;
            this.Function = function;
        }

        internal static ODataExpression FromReference(string reference)
        {
            return new ODataExpression(reference);
        }

        internal static ODataExpression FromValue(object value)
        {
            return new ODataExpression(value);
        }

        internal static ODataExpression FromAssignment(string reference, object value)
        {
            return new ODataExpression(reference, value);
        }

        internal static ODataExpression FromFunction(ExpressionFunction function)
        {
            return new ODataExpression(function);
        }

        internal static ODataExpression FromFunction(string functionName, string targetName, IEnumerable<object> arguments)
        {
            return new ODataExpression(
                new ODataExpression(targetName),
                new ExpressionFunction(functionName, arguments));
        }

        internal static ODataExpression FromLinqExpression(Expression expression)
        {
            return ParseLinqExpression(expression);
        }

        public override string ToString()
        {
            return Format(new ExpressionContext());
        }

        public string AsString()
        {
            return ToString();
        }

        internal bool ExtractEqualityComparisons(IDictionary<string, object> columnEqualityComparisons)
        {
            switch (_operator)
            {
                case ExpressionOperator.AND:
                    _left.ExtractEqualityComparisons(columnEqualityComparisons);
                    _right.ExtractEqualityComparisons(columnEqualityComparisons);
                    return true;

                case ExpressionOperator.EQ:
                    if (!string.IsNullOrEmpty(_left.Reference))
                    {
                        var key = _left.ToString().Split('.').Last();
                        if (!columnEqualityComparisons.ContainsKey(key))
                            columnEqualityComparisons.Add(key, _right);
                    }
                    return true;

                default:
                    return false;
            }
        }
    }
}
