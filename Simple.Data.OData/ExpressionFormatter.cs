using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.OData.Schema;

namespace Simple.Data.OData
{
    using System.Collections;
    using NExtLib;

    public class ExpressionFormatter
    {
        private readonly Dictionary<SimpleExpressionType, Func<SimpleExpression, string>> _expressionFormatters;
        private readonly SimpleReferenceFormatter _simpleReferenceFormatter;
        private readonly Func<string, Table> _findTable; 

        public ExpressionFormatter(Func<string, Table> findTable)
        {
            _expressionFormatters = new Dictionary<SimpleExpressionType, Func<SimpleExpression, string>>
                                        {
                                            { SimpleExpressionType.And, LogicalExpressionToWhereClause },
                                            { SimpleExpressionType.Or, LogicalExpressionToWhereClause },
                                            { SimpleExpressionType.Equal, EqualExpressionToWhereClause },
                                            { SimpleExpressionType.NotEqual, NotEqualExpressionToWhereClause },
                                            {
                                                SimpleExpressionType.GreaterThan,
                                                expr => BinaryExpressionToWhereClause(expr, "gt")
                                                },
                                            {
                                                SimpleExpressionType.GreaterThanOrEqual,
                                                expr => BinaryExpressionToWhereClause(expr, "ge")
                                                },
                                            {
                                                SimpleExpressionType.LessThan,
                                                expr => BinaryExpressionToWhereClause(expr, "lt")
                                                },
                                            {
                                                SimpleExpressionType.LessThanOrEqual,
                                                expr => BinaryExpressionToWhereClause(expr, "le")
                                                },
                                            { SimpleExpressionType.Empty, expr => string.Empty },
                                        };
            _findTable = findTable;
            _simpleReferenceFormatter = new SimpleReferenceFormatter(findTable);
        }

        public string Format(SimpleExpression expression)
        {
            Func<SimpleExpression, string> formatter;

            if (expression != null && _expressionFormatters.TryGetValue(expression.Type, out formatter))
            {
                return formatter(expression);
            }

            return string.Empty;
        }

        public string Format(IDictionary<string, object> keyValues)
        {
            if (keyValues.Count() == 1)
            {
                return FormatValue(keyValues.First().Value);
            }
            else
            {
                return string.Join(",", keyValues.Select(x => string.Format("{0}={1}", x.Key, FormatValue(x.Value))));
            }
        }

        private string LogicalExpressionToWhereClause(SimpleExpression expression)
        {
            return string.Format("({0} {1} {2})",
                                 Format((SimpleExpression) expression.LeftOperand),
                                 expression.Type.ToString().ToLowerInvariant(),
                                 Format((SimpleExpression) expression.RightOperand));
        }

        private string EqualExpressionToWhereClause(SimpleExpression expression)
        {
            if (expression.RightOperand == null)
                return FormatAsComparison(expression, "eq");
            if (CommonTypes.Contains(expression.RightOperand.GetType())) 
                return FormatAsComparison(expression, "eq");

            return TryFormatAsRange(expression.LeftOperand, expression.RightOperand as IRange)
                ?? FormatAsComparison(expression, "eq");
        }

        private string NotEqualExpressionToWhereClause(SimpleExpression expression)
        {
            if (expression.RightOperand == null)
                return FormatAsComparison(expression, "ne");
            if (CommonTypes.Contains(expression.RightOperand.GetType())) 
                return FormatAsComparison(expression, "ne");

            return FormatAsComparison(expression, "ne");
        }

        private string FormatAsComparison(SimpleExpression expression, string op)
        {
            return string.Format("{0} {1} {2}", FormatObject(expression.LeftOperand), op,
                                 FormatObject(expression.RightOperand));
        }

        private string TryFormatAsRange(object leftOperand, IRange range)
        {
            return (range != null)
                       ? string.Format("({0} ge {1} and {0} le {2})",
                                 _simpleReferenceFormatter.FormatColumnClause(leftOperand as SimpleReference),
                                 FormatObject(range.Start), FormatObject(range.End))
                       : null;
        }

        private string BinaryExpressionToWhereClause(SimpleExpression expression, string comparisonOperator)
        {
            return string.Format("{0} {1} {2}", FormatObject(expression.LeftOperand),
                                 comparisonOperator,
                                 FormatObject(expression.RightOperand));
        }

        internal protected string FormatObject(object value)
        {
            if (value is SimpleFunction)
                return FormatFunction(value as SimpleFunction);

            var simpleReference = value as SimpleReference;
            if (!ReferenceEquals(simpleReference, null))
                return FormatReference(simpleReference);

            return FormatValue(value);
        }

        internal protected string FormatReference(SimpleReference reference)
        {
            return _simpleReferenceFormatter.FormatColumnClause(reference);
        }

        internal protected string FormatFunction(SimpleFunction function)
        {
            return string.Format("{0}({1})", function.Name, function.Args.Aggregate((x, y) => FormatObject(x) + "," + FormatObject(y)));
        }

        internal static string FormatValue(object value)
        {
            return value == null ? "null"
                : value is string ? string.Format("'{0}'", value)
                : value is DateTime ? ((DateTime)value).ToIso8601String()
                : value is bool ? ((bool)value) ? "true" : "false"
                : (value is long || value is ulong) ? value.ToString() + "L"
                : value.ToString();
        }
    }
}
