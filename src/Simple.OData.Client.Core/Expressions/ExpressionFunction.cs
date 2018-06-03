using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public class ExpressionFunction
    {
        public string FunctionName { get; set; }
        public List<ODataExpression> Arguments { get; set; }

        public class FunctionCall
        {
            public string FunctionName { get; private set; }
            public int ArgumentCount { get; private set; }

            public FunctionCall(string functionName, int argumentCount)
            {
                this.FunctionName = functionName;
                this.ArgumentCount = argumentCount;
            }

            public override bool Equals(object obj)
            {
                if (obj is FunctionCall)
                {
                    return this.FunctionName == (obj as FunctionCall).FunctionName &&
                           this.ArgumentCount == (obj as FunctionCall).ArgumentCount;
                }
                else
                {
                    return base.Equals(obj);
                }
            }

            public override int GetHashCode()
            {
                return this.FunctionName.GetHashCode() ^ this.ArgumentCount.GetHashCode();
            }
        }

        internal ExpressionFunction()
        {
        }

        public ExpressionFunction(string functionName, IEnumerable<object> arguments)
        {
            this.FunctionName = functionName;
            this.Arguments = arguments.Select(ODataExpression.FromValue).ToList();
        }

        public ExpressionFunction(string functionName, IEnumerable<Expression> arguments)
        {
            this.FunctionName = functionName;
            this.Arguments = arguments.Select(ODataExpression.FromLinqExpression).ToList();
        }
    }
}