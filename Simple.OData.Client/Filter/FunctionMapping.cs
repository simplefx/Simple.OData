using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.OData.Client
{
    internal class FunctionMapping
    {
        public string FunctionName { get; private set; }
        public Func<string, string, IEnumerable<object>, FilterExpression> FunctionMapper { get; private set; }

        public FunctionMapping(string functionName)
        {
            this.FunctionName = functionName;
            this.FunctionMapper = FunctionWithTarget;
        }

        public FunctionMapping(string functionName, Func<string, string, IEnumerable<object>, FilterExpression> functionMapper)
        {
            this.FunctionName = functionName;
            this.FunctionMapper = functionMapper;
        }

        private readonly static Func<string, string, IEnumerable<object>, FilterExpression> FunctionWithTarget =
                (functionName, targetName, arguments) => FilterExpression.FromFunction(
                new ExpressionFunction()
                {
                    FunctionName = SupportedFunctions[new ExpressionFunction.FunctionCall(functionName, 0)].FunctionName,
                    Arguments = new List<FilterExpression>() { FilterExpression.FromReference(targetName) },
                });

        private readonly static Func<string, string, IEnumerable<object>, FilterExpression> FunctionWithTargetAndArguments =
                (functionName, targetName, arguments) => FilterExpression.FromFunction(
                new ExpressionFunction()
                {
                    FunctionName = SupportedFunctions[new ExpressionFunction.FunctionCall(functionName, arguments.Count())].FunctionName,
                    Arguments = MergeArguments(FilterExpression.FromReference(targetName), arguments),
                });

        private readonly static Func<string, string, IEnumerable<object>, FilterExpression> FunctionWithArgumentsAndTarget =
                (functionName, targetName, arguments) => FilterExpression.FromFunction(
                new ExpressionFunction()
                {
                    FunctionName = SupportedFunctions[new ExpressionFunction.FunctionCall(functionName, arguments.Count())].FunctionName,
                    Arguments = MergeArguments(arguments, FilterExpression.FromReference(targetName)),
                });

        public static Dictionary<ExpressionFunction.FunctionCall, FunctionMapping> SupportedFunctions = new Dictionary<ExpressionFunction.FunctionCall, FunctionMapping>()
            {
                {new ExpressionFunction.FunctionCall("Contains", 1), new FunctionMapping("substringof", FunctionWithArgumentsAndTarget)},
                {new ExpressionFunction.FunctionCall("StartsWith", 1), new FunctionMapping("startswith", FunctionWithTargetAndArguments)},
                {new ExpressionFunction.FunctionCall("EndsWith", 1), new FunctionMapping("endswith", FunctionWithTargetAndArguments)},
                {new ExpressionFunction.FunctionCall("Length", 0), new FunctionMapping("length")},
                {new ExpressionFunction.FunctionCall("IndexOf", 1), new FunctionMapping("indexof", FunctionWithTargetAndArguments)},
                {new ExpressionFunction.FunctionCall("Replace", 2), new FunctionMapping("replace", FunctionWithTargetAndArguments)},
                {new ExpressionFunction.FunctionCall("Substring", 1), new FunctionMapping("substring", FunctionWithTargetAndArguments)},
                {new ExpressionFunction.FunctionCall("Substring", 2), new FunctionMapping("substring", FunctionWithTargetAndArguments)},
                {new ExpressionFunction.FunctionCall("ToLower", 0), new FunctionMapping("tolower")},
                {new ExpressionFunction.FunctionCall("ToUpper", 0), new FunctionMapping("toupper")},
                {new ExpressionFunction.FunctionCall("Trim", 0), new FunctionMapping("trim")},
                {new ExpressionFunction.FunctionCall("Concat", 1), new FunctionMapping("concat", FunctionWithTargetAndArguments)},
                {new ExpressionFunction.FunctionCall("Year", 0), new FunctionMapping("year")},
                {new ExpressionFunction.FunctionCall("Month", 0), new FunctionMapping("month")},
                {new ExpressionFunction.FunctionCall("Day", 0), new FunctionMapping("day")},
                {new ExpressionFunction.FunctionCall("Hour", 0), new FunctionMapping("hour")},
                {new ExpressionFunction.FunctionCall("Minute", 0), new FunctionMapping("minute")},
                {new ExpressionFunction.FunctionCall("Second", 0), new FunctionMapping("second")},
                {new ExpressionFunction.FunctionCall("Round", 0), new FunctionMapping("round")},
                {new ExpressionFunction.FunctionCall("Floor", 0), new FunctionMapping("floor")},
                {new ExpressionFunction.FunctionCall("Ceiling", 0), new FunctionMapping("ceiling")},
            };

        private static List<FilterExpression> MergeArguments(FilterExpression argument, IEnumerable<object> arguments)
        {
            var collection = new List<FilterExpression>();
            collection.Add(argument);
            collection.AddRange(arguments.Select(FilterExpression.FromValue));
            return collection;
        }

        private static List<FilterExpression> MergeArguments(IEnumerable<object> arguments, FilterExpression argument)
        {
            var collection = new List<FilterExpression>();
            collection.AddRange(arguments.Select(FilterExpression.FromValue));
            collection.Add(argument);
            return collection;
        }
    }
}