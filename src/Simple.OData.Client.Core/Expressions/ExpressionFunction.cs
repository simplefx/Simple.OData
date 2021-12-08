using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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
				FunctionName = functionName;
				ArgumentCount = argumentCount;
			}

			public override bool Equals(object obj)
			{
				if (obj is FunctionCall)
				{
					return FunctionName == (obj as FunctionCall).FunctionName &&
						   ArgumentCount == (obj as FunctionCall).ArgumentCount;
				}
				else
				{
					return base.Equals(obj);
				}
			}

			public override int GetHashCode()
			{
				return FunctionName.GetHashCode() ^ ArgumentCount.GetHashCode();
			}
		}

		internal ExpressionFunction()
		{
		}

		public ExpressionFunction(string functionName, IEnumerable<object> arguments)
		{
			FunctionName = functionName;
			Arguments = arguments.Select(ODataExpression.FromValue).ToList();
		}

		public ExpressionFunction(string functionName, IEnumerable<Expression> arguments)
		{
			FunctionName = functionName;
			Arguments = arguments.Select(ODataExpression.FromLinqExpression).ToList();
		}
	}
}