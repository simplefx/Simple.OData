using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public partial class ODataExpression
    {
        private readonly ODataExpression _functionCaller;
        private readonly ODataExpression _left;
        private readonly ODataExpression _right;
        private readonly ExpressionOperator _operator;
        private readonly Type _conversionType;

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

        protected ODataExpression(ODataExpression expr, Type conversionType)
        {
            _left = expr;
            _conversionType = conversionType;
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

        internal static ODataExpression FromFunction(string functionName, string targetName, IEnumerable<Expression> arguments)
        {
            return new ODataExpression(
                new ODataExpression(targetName),
                new ExpressionFunction(functionName, arguments));
        }

        internal static ODataExpression FromLinqExpression(Expression expression)
        {
            return ParseLinqExpression(expression);
        }

        public bool IsNull
        {
            get { return this.Value == null && 
                this.Reference == null && 
                this.Function == null && 
                _operator == ExpressionOperator.None; }
        }

        public string AsString(ISession session)
        {
            return Format(new ExpressionContext(session));
        }

        internal bool ExtractLookupColumns(IDictionary<string, object> lookupColumns)
        {
            switch (_operator)
            {
                case ExpressionOperator.AND:
                    var ok = _left.ExtractLookupColumns(lookupColumns);
                    if (ok)
                        ok = _right.ExtractLookupColumns(lookupColumns);
                    return ok;

                case ExpressionOperator.EQ:
                    var expr = _left;
                    while (expr._conversionType != null)
                    {
                        expr = expr._left;
                    }
                    if (!string.IsNullOrEmpty(expr.Reference))
                    {
                        var key = expr.Reference.Split('.', '/').Last();
                        if (key != null && !lookupColumns.ContainsKey(key))
                            lookupColumns.Add(key, _right);
                    }
                    return true;

                default:
                    if (_conversionType != null)
                    {
                        return _left.ExtractLookupColumns(lookupColumns);
                    }
                    else
                    {
                        return false;
                    }
            }
        }

        internal bool HasTypeConstraint(string typeName)
        {
            if (_operator == ExpressionOperator.AND)
            {
                return _left.HasTypeConstraint(typeName) || _right.HasTypeConstraint(typeName);
            }
            else if (this.Function != null && this.Function.FunctionName == ODataLiteral.IsOf)
            {
                return this.Function.Arguments.Last().HasTypeConstraint(typeName);
            }
            else if (this.Value != null)
            {
                return this.Value is Type && (this.Value as Type).Name == typeName;
            }
            else
            {
                return false;
            }
        }
    }
}
