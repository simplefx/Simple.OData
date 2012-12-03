using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Simple.OData.Client
{
    public class ExpressionFunction
    {
        public string FunctionName { get; set; }
        public List<FilterExpression> Arguments { get; set; }

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

        public class FunctionMapping
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
        }

        private ExpressionFunction()
        {
        }

        private readonly static Func<string, string, IEnumerable<object>, FilterExpression>
            FunctionWithTarget =
                (functionName, targetName, arguments) => FilterExpression.FromFunction(
                new ExpressionFunction()
                {
                    FunctionName = SupportedFunctions[new FunctionCall(functionName, 0)].FunctionName,
                    Arguments = new List<FilterExpression>() { FilterExpression.FromReference(targetName) },
                });

        private readonly static Func<string, string, IEnumerable<object>, FilterExpression>
            FunctionWithTargetAndArguments =
                (functionName, targetName, arguments) => FilterExpression.FromFunction(
                new ExpressionFunction()
                {
                    FunctionName = SupportedFunctions[new FunctionCall(functionName, arguments.Count())].FunctionName,
                    Arguments = MergeArguments(FilterExpression.FromReference(targetName), arguments),
                });

        private readonly static Func<string, string, IEnumerable<object>, FilterExpression>
            FunctionWithArgumentsAndTarget =
                (functionName, targetName, arguments) => FilterExpression.FromFunction(
                new ExpressionFunction()
                {
                    FunctionName = SupportedFunctions[new FunctionCall(functionName, arguments.Count())].FunctionName,
                    Arguments = MergeArguments(arguments, FilterExpression.FromReference(targetName)),
                });

        public static Dictionary<FunctionCall, FunctionMapping> SupportedFunctions = new Dictionary<FunctionCall, FunctionMapping>()
            {
                {new FunctionCall("Contains", 1), new FunctionMapping("substringof", FunctionWithArgumentsAndTarget)},
                {new FunctionCall("StartsWith", 1), new FunctionMapping("startswith", FunctionWithTargetAndArguments)},
                {new FunctionCall("EndsWith", 1), new FunctionMapping("endswith", FunctionWithTargetAndArguments)},
                {new FunctionCall("Length", 0), new FunctionMapping("length")},
                {new FunctionCall("IndexOf", 1), new FunctionMapping("indexof", FunctionWithTargetAndArguments)},
                {new FunctionCall("Replace", 2), new FunctionMapping("replace", FunctionWithTargetAndArguments)},
                {new FunctionCall("Substring", 1), new FunctionMapping("substring", FunctionWithTargetAndArguments)},
                {new FunctionCall("Substring", 2), new FunctionMapping("substring", FunctionWithTargetAndArguments)},
                {new FunctionCall("ToLower", 0), new FunctionMapping("tolower")},
                {new FunctionCall("ToUpper", 0), new FunctionMapping("toupper")},
                {new FunctionCall("Trim", 0), new FunctionMapping("trim")},
                {new FunctionCall("Concat", 1), new FunctionMapping("concat", FunctionWithTargetAndArguments)},
                {new FunctionCall("Year", 0), new FunctionMapping("year")},
                {new FunctionCall("Month", 0), new FunctionMapping("month")},
                {new FunctionCall("Day", 0), new FunctionMapping("day")},
                {new FunctionCall("Hour", 0), new FunctionMapping("hour")},
                {new FunctionCall("Minute", 0), new FunctionMapping("minute")},
                {new FunctionCall("Second", 0), new FunctionMapping("second")},
                {new FunctionCall("Round", 0), new FunctionMapping("round")},
                {new FunctionCall("Floor", 0), new FunctionMapping("floor")},
                {new FunctionCall("Ceiling", 0), new FunctionMapping("ceiling")},
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