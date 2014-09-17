using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.OData.Client
{
    internal class FunctionMapping
    {
        public class FunctionDefinition
        {
            public FunctionDefinition(ExpressionFunction.FunctionCall functionCall, FunctionMapping functionMapping, AdapterVersion adapterVersion = AdapterVersion.Any)
            {
                FunctionCall = functionCall;
                FunctionMapping = functionMapping;
                AdapterVersion = adapterVersion;
            }

            public ExpressionFunction.FunctionCall FunctionCall { get; set; }
            public FunctionMapping FunctionMapping { get; set; }
            public AdapterVersion AdapterVersion { get; set; }
        }

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
                    FunctionName = GetFunction(functionName, 0).FunctionMapping.FunctionName,
                    Arguments = new List<ODataExpression>() { ODataExpression.FromReference(targetName) },
                });

        private readonly static Func<string, string, IEnumerable<object>, ODataExpression> FunctionWithTargetAndArguments =
                (functionName, targetName, arguments) => ODataExpression.FromFunction(
                new ExpressionFunction()
                {
                    FunctionName = GetFunction(functionName, arguments.Count()).FunctionMapping.FunctionName,
                    Arguments = MergeArguments(ODataExpression.FromReference(targetName), arguments),
                });

        private readonly static Func<string, string, IEnumerable<object>, ODataExpression> FunctionWithArgumentsAndTarget =
                (functionName, targetName, arguments) => ODataExpression.FromFunction(
                new ExpressionFunction()
                {
                    FunctionName = GetFunction(functionName, arguments.Count()).FunctionMapping.FunctionName,
                    Arguments = MergeArguments(arguments, ODataExpression.FromReference(targetName)),
                });

        public static readonly FunctionDefinition[] DefinedFunctions =
            {
                new FunctionDefinition(new ExpressionFunction.FunctionCall("Contains", 1), new FunctionMapping("substringof", FunctionWithArgumentsAndTarget), AdapterVersion.V3),
                new FunctionDefinition(new ExpressionFunction.FunctionCall("Contains", 1), new FunctionMapping("contains", FunctionWithTargetAndArguments), AdapterVersion.V4),
                new FunctionDefinition(new ExpressionFunction.FunctionCall("StartsWith", 1), new FunctionMapping("startswith", FunctionWithTargetAndArguments)),
                new FunctionDefinition(new ExpressionFunction.FunctionCall("EndsWith", 1), new FunctionMapping("endswith", FunctionWithTargetAndArguments)),
                new FunctionDefinition(new ExpressionFunction.FunctionCall("Length", 0), new FunctionMapping("length")),
                new FunctionDefinition(new ExpressionFunction.FunctionCall("IndexOf", 1), new FunctionMapping("indexof", FunctionWithTargetAndArguments)),
                new FunctionDefinition(new ExpressionFunction.FunctionCall("Replace", 2), new FunctionMapping("replace", FunctionWithTargetAndArguments)),
                new FunctionDefinition(new ExpressionFunction.FunctionCall("Substring", 1), new FunctionMapping("substring", FunctionWithTargetAndArguments)),
                new FunctionDefinition(new ExpressionFunction.FunctionCall("Substring", 2), new FunctionMapping("substring", FunctionWithTargetAndArguments)),
                new FunctionDefinition(new ExpressionFunction.FunctionCall("ToLower", 0), new FunctionMapping("tolower")),
                new FunctionDefinition(new ExpressionFunction.FunctionCall("ToUpper", 0), new FunctionMapping("toupper")),
                new FunctionDefinition(new ExpressionFunction.FunctionCall("Trim", 0), new FunctionMapping("trim")),
                new FunctionDefinition(new ExpressionFunction.FunctionCall("Concat", 1), new FunctionMapping("concat", FunctionWithTargetAndArguments)),
                new FunctionDefinition(new ExpressionFunction.FunctionCall("Year", 0), new FunctionMapping("year")),
                new FunctionDefinition(new ExpressionFunction.FunctionCall("Month", 0), new FunctionMapping("month")),
                new FunctionDefinition(new ExpressionFunction.FunctionCall("Day", 0), new FunctionMapping("day")),
                new FunctionDefinition(new ExpressionFunction.FunctionCall("Hour", 0), new FunctionMapping("hour")),
                new FunctionDefinition(new ExpressionFunction.FunctionCall("Minute", 0), new FunctionMapping("minute")),
                new FunctionDefinition(new ExpressionFunction.FunctionCall("Second", 0), new FunctionMapping("second")),
                new FunctionDefinition(new ExpressionFunction.FunctionCall("Milliseconds", 0), new FunctionMapping("fractionalseconds"), AdapterVersion.V4),
                new FunctionDefinition(new ExpressionFunction.FunctionCall("Date", 0), new FunctionMapping("date"), AdapterVersion.V4),
                new FunctionDefinition(new ExpressionFunction.FunctionCall("Time", 0), new FunctionMapping("time"), AdapterVersion.V4),
                new FunctionDefinition(new ExpressionFunction.FunctionCall("Totaloffsetminutes", 0), new FunctionMapping("totaloffsetminutes"), AdapterVersion.V4),
                new FunctionDefinition(new ExpressionFunction.FunctionCall("Now", 0), new FunctionMapping("now"), AdapterVersion.V4),
                new FunctionDefinition(new ExpressionFunction.FunctionCall("Maxdatetime", 0), new FunctionMapping("maxdatetime"), AdapterVersion.V4),
                new FunctionDefinition(new ExpressionFunction.FunctionCall("Mindatetime", 0), new FunctionMapping("mindatetime"), AdapterVersion.V4),
                new FunctionDefinition(new ExpressionFunction.FunctionCall("Totalseconds", 0), new FunctionMapping("totalseconds"), AdapterVersion.V4),
                new FunctionDefinition(new ExpressionFunction.FunctionCall("Round", 0), new FunctionMapping("round")),
                new FunctionDefinition(new ExpressionFunction.FunctionCall("Floor", 0), new FunctionMapping("floor")),
                new FunctionDefinition(new ExpressionFunction.FunctionCall("Ceiling", 0), new FunctionMapping("ceiling")),
            };

        public static bool ContainsFunction(string functionName, int argumentCount)
        {
            return DefinedFunctions.Any(x => x.FunctionCall.Equals(new ExpressionFunction.FunctionCall(functionName, argumentCount)));
        }

        public static FunctionDefinition GetFunction(string functionName, int argumentCount)
        {
            return DefinedFunctions.SingleOrDefault(x => x.FunctionCall.Equals(new ExpressionFunction.FunctionCall(functionName, argumentCount)));
        }

        public static bool TryGetFunctionMapping(string functionName, int argumentCount, AdapterVersion adapterVersion, out FunctionMapping functionMapping)
        {
            functionMapping = null;
            var function = DefinedFunctions.SingleOrDefault(x => 
                x.FunctionCall.Equals(new ExpressionFunction.FunctionCall(functionName, argumentCount)) && 
                (x.AdapterVersion & adapterVersion) == adapterVersion);

            if (function != null)
            {
                functionMapping = function.FunctionMapping;
            }
            return function != null;
        }

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