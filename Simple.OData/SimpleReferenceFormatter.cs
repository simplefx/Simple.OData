using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data;
using Simple.NExtLib;

namespace Simple.OData
{
    public class SimpleReferenceFormatter
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
