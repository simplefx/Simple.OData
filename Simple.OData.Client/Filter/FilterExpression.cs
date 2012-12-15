using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Simple.OData.Client
{
    public partial class FilterExpression : DynamicObject
    {
        private string _reference;
        private object _value;
        private ExpressionFunction _function;
        private FilterExpression _functionCaller;
        private readonly FilterExpression _left;
        private readonly FilterExpression _right;
        private readonly ExpressionOperator _operator;

        public FilterExpression()
        {
        }

        private FilterExpression(FilterExpression left, FilterExpression right, ExpressionOperator expressionOperator)
        {
            _left = left;
            _right = right;
            _operator = expressionOperator;
        }

        internal static FilterExpression FromReference(string reference)
        {
            return new FilterExpression { _reference = reference };
        }

        internal static FilterExpression FromValue(object value)
        {
            return new FilterExpression { _value = value };
        }

        internal static FilterExpression FromFunction(ExpressionFunction function)
        {
            return new FilterExpression { _function = function };
        }

        internal static FilterExpression FromFunction(string functionName, string targetName, IEnumerable<object> arguments)
        {
            var expression = new FilterExpression();
            expression._functionCaller = FilterExpression.FromReference(targetName);
            expression._function = new ExpressionFunction(functionName, arguments);
            return expression;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            FunctionMapping mapping;
            if (FunctionMapping.SupportedFunctions.TryGetValue(new ExpressionFunction.FunctionCall(binder.Name, 0), out mapping))
            {
                result = new FilterExpression() {_functionCaller = this, _reference = binder.Name};
                return true;
            }
            return base.TryGetMember(binder, out result);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            FunctionMapping mapping;
            if (FunctionMapping.SupportedFunctions.TryGetValue(new ExpressionFunction.FunctionCall(binder.Name, args.Count()), out mapping))
            {
                result = new FilterExpression() { _functionCaller = this, _function = new ExpressionFunction(binder.Name, args) };
                return true;
            }
            return base.TryInvokeMember(binder, args, out result);
        }

        public override string ToString()
        {
            return Format(new ExpressionContext());
        }

        internal void ExtractEqualityComparisons(IDictionary<string, object> columnEqualityComparisons)
        {
            switch (_operator)
            {
                case ExpressionOperator.AND:
                    _left.ExtractEqualityComparisons(columnEqualityComparisons);
                    _right.ExtractEqualityComparisons(columnEqualityComparisons);
                    break;

                case ExpressionOperator.EQ:
                    if (!string.IsNullOrEmpty(_left._reference))
                    {
                        var key = _left.ToString().Split('.').Last();
                        if (!columnEqualityComparisons.ContainsKey(key))
                            columnEqualityComparisons.Add(key, _right);
                    }
                    break;
            }
        }
    }
}
