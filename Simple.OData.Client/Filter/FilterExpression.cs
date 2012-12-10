using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Simple.OData.Client
{
    public partial class FilterExpression : DynamicObject
    {
        internal class ExpressionContext
        {
            public ODataClientWithCommand Client { get; set; }
            public Table Table { get; set; }

            public bool IsSet
            {
                get { return this.Client != null && this.Table != null; }
            }
        }

        private FilterExpression _parent;
        private string _reference;
        private ExpressionFunction _function;
        private object _value;
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
            expression._function = new ExpressionFunction(functionName, FilterExpression.FromReference(targetName), arguments);
            return expression;
            //ExpressionFunction.FunctionMapping mapping;
            //if (ExpressionFunction.SupportedFunctions.TryGetValue(new ExpressionFunction.FunctionCall(functionName, arguments == null ? 0 : arguments.Count()), out mapping))
            //{
            //    return mapping.FunctionMapper(functionName, targetName, arguments);
            //}
            //else
            //{
            //    throw new NotSupportedException(string.Format("The function {0} is not supported or called with wrong number of arguments", functionName));
            //}
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            ExpressionFunction.FunctionMapping mapping;
            if (ExpressionFunction.SupportedFunctions.TryGetValue(new ExpressionFunction.FunctionCall(binder.Name, 0), out mapping))
            {
//                result = mapping.FunctionMapper(binder.Name, this.Format(), null);
                result = new FilterExpression() {_parent = this, _reference = binder.Name};
                return true;
            }
            return base.TryGetMember(binder, out result);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            ExpressionFunction.FunctionMapping mapping;
            if (ExpressionFunction.SupportedFunctions.TryGetValue(new ExpressionFunction.FunctionCall(binder.Name, args.Count()), out mapping))
            {
//                result = mapping.FunctionMapper(binder.Name, this.Format(), args);
                var expression = new FilterExpression();
                expression._function = new ExpressionFunction(binder.Name, this, args);
                result = expression;
                return true;
            }
            return base.TryInvokeMember(binder, args, out result);
        }

        //public override string ToString()
        //{
        //    return Format();
        //}
    }
}
