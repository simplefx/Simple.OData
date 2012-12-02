using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Simple.OData.Client
{
    class ExpressionFunction
    {
        public string Name { get; set; }
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
            public Func<string, FilterExpression, IEnumerable<object>, FilterExpression> FunctionMapper { get; private set; }

            public FunctionMapping(string functionName)
            {
                this.FunctionName = functionName;
                this.FunctionMapper = FunctionWithSource;
            }

            public FunctionMapping(string functionName, Func<string, FilterExpression, IEnumerable<object>, FilterExpression> functionMapper)
            {
                this.FunctionName = functionName;
                this.FunctionMapper = functionMapper;
            }
        }

        private ExpressionFunction()
        {
        }

        private readonly static Func<string, FilterExpression, IEnumerable<object>, FilterExpression>
            FunctionWithSource =
                (name, source, arguments) => FilterExpression.FromFunction(
                new ExpressionFunction()
                {
                    Name = SupportedFunctions[new FunctionCall(name, 0)].FunctionName,
                    Arguments = new List<FilterExpression>() { source },
                });

        private readonly static Func<string, FilterExpression, IEnumerable<object>, FilterExpression>
            FunctionWithSourceAndArguments =
                (name, source, arguments) => FilterExpression.FromFunction(
                new ExpressionFunction()
                {
                    Name = SupportedFunctions[new FunctionCall(name, arguments.Count())].FunctionName,
                    Arguments = MergeArguments(source, arguments),
                });

        private readonly static Func<string, FilterExpression, IEnumerable<object>, FilterExpression>
            FunctionWithArgumentsAndSource =
                (name, source, arguments) => FilterExpression.FromFunction(
                new ExpressionFunction()
                {
                    Name = SupportedFunctions[new FunctionCall(name, arguments.Count())].FunctionName,
                    Arguments = MergeArguments(arguments, source),
                });

        public static Dictionary<FunctionCall, FunctionMapping> SupportedFunctions = new Dictionary<FunctionCall, FunctionMapping>()
            {
                {new FunctionCall("Contains", 1), new FunctionMapping("substringof", FunctionWithArgumentsAndSource)},
                {new FunctionCall("StartsWith", 1), new FunctionMapping("startswith", FunctionWithSourceAndArguments)},
                {new FunctionCall("EndsWith", 1), new FunctionMapping("endswith", FunctionWithSourceAndArguments)},
                {new FunctionCall("Length", 0), new FunctionMapping("length")},
                {new FunctionCall("IndexOf", 1), new FunctionMapping("indexof", FunctionWithSourceAndArguments)},
                {new FunctionCall("Replace", 2), new FunctionMapping("replace", FunctionWithSourceAndArguments)},
                {new FunctionCall("Substring", 1), new FunctionMapping("substring", FunctionWithSourceAndArguments)},
                {new FunctionCall("Substring", 2), new FunctionMapping("substring", FunctionWithSourceAndArguments)},
                {new FunctionCall("ToLower", 0), new FunctionMapping("tolower")},
                {new FunctionCall("ToUpper", 0), new FunctionMapping("toupper")},
                {new FunctionCall("Trim", 0), new FunctionMapping("trim")},
                {new FunctionCall("Concat", 1), new FunctionMapping("concat", FunctionWithSourceAndArguments)},
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