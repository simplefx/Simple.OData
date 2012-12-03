using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Simple.OData.Client
{
    public partial class FilterExpression : DynamicObject
    {
        private string _reference;
        private ExpressionFunction _function;
        private object _value;
        private readonly FilterExpression _left;
        private readonly FilterExpression _right;
        private readonly ExpressionOperator _operator;

        private FilterExpression()
        {
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
            ExpressionFunction.FunctionMapping mapping;
            if (ExpressionFunction.SupportedFunctions.TryGetValue(new ExpressionFunction.FunctionCall(functionName, arguments == null ? 0 : arguments.Count()), out mapping))
            {
                return mapping.FunctionMapper(functionName, targetName, arguments);
            }
            else
            {
                throw new NotSupportedException(string.Format("The function {0} is not supported or called with wrong number of arguments", functionName));
            }
        }

        private FilterExpression(FilterExpression left, FilterExpression right, ExpressionOperator expressionOperator)
        {
            _left = left;
            _right = right;
            _operator = expressionOperator;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            ExpressionFunction.FunctionMapping mapping;
            if (ExpressionFunction.SupportedFunctions.TryGetValue(new ExpressionFunction.FunctionCall(binder.Name, 0), out mapping))
            {
                result = mapping.FunctionMapper(binder.Name, this, null);
                return true;
            }
            return base.TryGetMember(binder, out result);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            ExpressionFunction.FunctionMapping mapping;
            if (ExpressionFunction.SupportedFunctions.TryGetValue(new ExpressionFunction.FunctionCall(binder.Name, args.Count()), out mapping))
            {
                result = mapping.FunctionMapper(binder.Name, this, args);
                return true;
            }
            return base.TryInvokeMember(binder, args, out result);
        }
    }
}
