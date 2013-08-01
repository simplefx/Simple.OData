using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Simple.OData.Client
{
    public partial class FilterExpression : DynamicObject
    {
        private readonly FilterExpression _functionCaller;
        private readonly FilterExpression _left;
        private readonly FilterExpression _right;
        private readonly ExpressionOperator _operator;

        public string Reference { get; private set; }
        public object Value { get; private set; }
        public ExpressionFunction Function { get; private set; }

        internal FilterExpression()
        {
        }

        private FilterExpression(object value)
        {
            this.Value = value;
        }

        private FilterExpression(string reference)
        {
            this.Reference = reference;
        }

        private FilterExpression(ExpressionFunction function)
        {
            this.Function = function;
        }

        private FilterExpression(FilterExpression left, FilterExpression right, ExpressionOperator expressionOperator)
        {
            _left = left;
            _right = right;
            _operator = expressionOperator;
        }

        private FilterExpression(FilterExpression caller, string reference)
        {
            _functionCaller = caller;
            this.Reference = reference;
        }

        private FilterExpression(FilterExpression caller, ExpressionFunction function)
        {
            _functionCaller = caller;
            this.Function = function;
        }

        internal static FilterExpression FromReference(string reference)
        {
            return new FilterExpression(reference);
        }

        internal static FilterExpression FromValue(object value)
        {
            return new FilterExpression(value);
        }

        internal static FilterExpression FromFunction(ExpressionFunction function)
        {
            return new FilterExpression(function);
        }

        internal static FilterExpression FromFunction(string functionName, string targetName, IEnumerable<object> arguments)
        {
            return new FilterExpression(
                new FilterExpression(targetName),
                new ExpressionFunction(functionName, arguments));
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            FunctionMapping mapping;
            if (FunctionMapping.SupportedFunctions.TryGetValue(new ExpressionFunction.FunctionCall(binder.Name, 0), out mapping))
            {
                result = new FilterExpression(this, binder.Name);
            }
            else
            {
                result = new FilterExpression(binder.Name);
            }
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            FunctionMapping mapping;
            if (FunctionMapping.SupportedFunctions.TryGetValue(new ExpressionFunction.FunctionCall(binder.Name, args.Count()), out mapping))
            {
                result = new FilterExpression(this, new ExpressionFunction(binder.Name, args));
                return true;
            }
            return base.TryInvokeMember(binder, args, out result);
        }

        public override string ToString()
        {
            return Format(new ExpressionContext());
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
