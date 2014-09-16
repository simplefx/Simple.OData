using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.OData.Client
{
    internal class FunctionMapping
    {
        public string FunctionName { get; private set; }
        public Func<string, string, IEnumerable<object>, ODataExpression> FunctionMapper { get; private set; }

        public FunctionMapping(string functionName)
        {
            this.FunctionName = functionName;
            this.FunctionMapper = FunctionWithTarget;
        }

        public FunctionMapping(string functionName, Func<string, string, IEnumerable<object>, ODataExpression> functionMapper)
        {
            this.FunctionName = functionName;
            this.FunctionMapper = functionMapper;
        }

        private readonly static Func<string, string, IEnumerable<object>, ODataExpression> FunctionWithTarget =
                (functionName, targetName, arguments) => ODataExpression.FromFunction(
                new ExpressionFunction()
                {
                    FunctionName = SupportedFunctions[new ExpressionFunction.FunctionCall(functionName, 0)].FunctionName,
                    Arguments = new List<ODataExpression>() { ODataExpression.FromReference(targetName) },
                });

        private readonly static Func<string, string, IEnumerable<object>, ODataExpression> FunctionWithTargetAndArguments =
                (functionName, targetName, arguments) => ODataExpression.FromFunction(
                new ExpressionFunction()
                {
                    FunctionName = SupportedFunctions[new ExpressionFunction.FunctionCall(functionName, arguments.Count())].FunctionName,
                    Arguments = MergeArguments(ODataExpression.FromReference(targetName), arguments),
                });

        private readonly static Func<string, string, IEnumerable<object>, ODataExpression> FunctionWithArgumentsAndTarget =
                (functionName, targetName, arguments) => ODataExpression.FromFunction(
                new ExpressionFunction()
                {
                    FunctionName = SupportedFunctions[new ExpressionFunction.FunctionCall(functionName, arguments.Count())].FunctionName,
                    Arguments = MergeArguments(arguments, ODataExpression.FromReference(targetName)),
                });

        public static Dictionary<ExpressionFunction.FunctionCall, FunctionMapping> SupportedFunctions = new Dictionary<ExpressionFunction.FunctionCall, FunctionMapping>()
            {
                {new ExpressionFunction.FunctionCall("Contains", 1), new FunctionMapping("substringof", FunctionWithArgumentsAndTarget)},
                //{new ExpressionFunction.FunctionCall("Contains", 1), new FunctionMapping("contains", FunctionWithTargetAndArguments)},
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
                //{new ExpressionFunction.FunctionCall("Milliseconds", 0), new FunctionMapping("fractionalseconds")},
                //{new ExpressionFunction.FunctionCall("Date", 0), new FunctionMapping("date")},
                //{new ExpressionFunction.FunctionCall("Time", 0), new FunctionMapping("time")},
                //{new ExpressionFunction.FunctionCall("Totaloffsetminutes", 0), new FunctionMapping("totaloffsetminutes")},
                //{new ExpressionFunction.FunctionCall("Now", 0), new FunctionMapping("now")},
                //{new ExpressionFunction.FunctionCall("Maxdatetime", 0), new FunctionMapping("maxdatetime")},
                //{new ExpressionFunction.FunctionCall("Mindatetime", 0), new FunctionMapping("mindatetime")},
                //{new ExpressionFunction.FunctionCall("Totalseconds", 0), new FunctionMapping("totalseconds")},
                {new ExpressionFunction.FunctionCall("Round", 0), new FunctionMapping("round")},
                {new ExpressionFunction.FunctionCall("Floor", 0), new FunctionMapping("floor")},
                {new ExpressionFunction.FunctionCall("Ceiling", 0), new FunctionMapping("ceiling")},
            };

        private static List<ODataExpression> MergeArguments(ODataExpression argument, IEnumerable<object> arguments)
        {
            var collection = new List<ODataExpression>();
            collection.Add(argument);
            collection.AddRange(arguments.Select(ODataExpression.FromValue));
            return collection;
        }

        private static List<ODataExpression> MergeArguments(IEnumerable<object> arguments, ODataExpression argument)
        {
            var collection = new List<ODataExpression>();
            collection.AddRange(arguments.Select(ODataExpression.FromValue));
            collection.Add(argument);
            return collection;
        }
    }
}