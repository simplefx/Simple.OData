using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Azure
{
    using System.Collections;
    using NExtLib;

    public class ExpressionFormatter
    {
        private readonly Dictionary<SimpleExpressionType, Func<SimpleExpression, string>> _expressionFormatters;
        private readonly SimpleReferenceFormatter _simpleReferenceFormatter = new SimpleReferenceFormatter();

        public ExpressionFormatter()
        {
            _expressionFormatters = new Dictionary<SimpleExpressionType, Func<SimpleExpression, string>>
                                        {
                                            { SimpleExpressionType.And, LogicalExpressionToWhereClause },
                                            { SimpleExpressionType.Or, LogicalExpressionToWhereClause },
                                            { SimpleExpressionType.Equal, EqualExpressionToWhereClause },
                                            { SimpleExpressionType.NotEqual, NotEqualExpressionToWhereClause },
                                            { SimpleExpressionType.Function, FunctionExpressionToWhereClause },
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
        }

        public string Format(SimpleExpression expression)
        {
            Func<SimpleExpression, string> formatter;

            if (_expressionFormatters.TryGetValue(expression.Type, out formatter))
            {
                return formatter(expression);
            }

            return string.Empty;
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
                return string.Format("not({0} ge '')", FormatObject(expression.LeftOperand));
            if (CommonTypes.Contains(expression.RightOperand.GetType())) return FormatAsComparison(expression, "eq");

            return TryFormatAsRange(expression.LeftOperand, expression.RightOperand as IRange)
                ?? FormatAsComparison(expression, "eq");
        }

        private string NotEqualExpressionToWhereClause(SimpleExpression expression)
        {
            if (expression.RightOperand == null)
                return string.Format("({0} ge '')", FormatObject(expression.LeftOperand));
            if (CommonTypes.Contains(expression.RightOperand.GetType())) return FormatAsComparison(expression, "ne");

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

        private string FunctionExpressionToWhereClause(SimpleExpression expression)
        {
            var function = expression.RightOperand as SimpleFunction;
            if (function == null) throw new InvalidOperationException("Expected SimpleFunction as the right operand.");

            if (function.Name.Equals("like", StringComparison.InvariantCultureIgnoreCase))
            {
                return string.Format("{0} LIKE {1}", FormatObject(expression.LeftOperand),
                                     FormatObject(function.Args[0]));
            }

            if (function.Name.Equals("notlike", StringComparison.InvariantCultureIgnoreCase))
            {
                return string.Format("{0} NOT LIKE {1}", FormatObject(expression.LeftOperand),
                                     FormatObject(function.Args[0]));
            }

            throw new NotSupportedException(string.Format("Unknown function '{0}'.", function.Name));
        }

        private string BinaryExpressionToWhereClause(SimpleExpression expression, string comparisonOperator)
        {
            return string.Format("{0} {1} {2}", FormatObject(expression.LeftOperand),
                                 comparisonOperator,
                                 FormatObject(expression.RightOperand));
        }

        protected string FormatObject(object value)
        {
            var objectReference = value as SimpleReference;

            if (!ReferenceEquals(objectReference, null))
                return _simpleReferenceFormatter.FormatColumnClause(objectReference);

            return value is string ? string.Format("'{0}'", value) : value is DateTime ? ((DateTime)value).ToIso8601String() : value.ToString();
        }
    }

    class SimpleReferenceFormatter
    {
        public string FormatColumnClause(SimpleReference reference)
        {
            var formatted = TryFormatAsObjectReference(reference as ObjectReference)
                            ??
                            TryFormatAsMathReference(reference as MathReference);

            if (formatted != null) return formatted;

            throw new InvalidOperationException("SimpleReference type not supported.");
        }

        private string FormatObject(object value)
        {
            var reference = value as SimpleReference;
            if (reference != null) return FormatColumnClause(reference);
            return value is string ? string.Format("'{0}'", value) : value is DateTime ? ((DateTime)value).ToIso8601String() : value.ToString();
        }

        private string TryFormatAsMathReference(MathReference mathReference)
        {
            if (ReferenceEquals(mathReference, null)) return null;

            return string.Format("{0} {1} {2}", FormatObject(mathReference.LeftOperand),
                                 MathOperatorToString(mathReference.Operator), FormatObject(mathReference.RightOperand));
        }

        private static string MathOperatorToString(MathOperator @operator)
        {
            switch (@operator)
            {
                case MathOperator.Add:
                    return "add";
                case MathOperator.Subtract:
                    return "sub";
                case MathOperator.Multiply:
                    return "mul";
                case MathOperator.Divide:
                    return "div";
                case MathOperator.Modulo:
                    return "mod";
                default:
                    throw new InvalidOperationException("Invalid MathOperator specified.");
            }
        }

        private string TryFormatAsObjectReference(ObjectReference objectReference)
        {
            if (ReferenceEquals(objectReference, null)) return null;

            return objectReference.GetName();
        }

    }
}
