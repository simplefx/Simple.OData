using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data;
using Simple.NExtLib;
using Simple.OData.Client;

namespace Simple.Data.OData
{
    public class SimpleReferenceFormatter
    {
        private readonly FunctionNameConverter _functionNameConverter = new FunctionNameConverter();
        private readonly Func<string, Table> _findTable;

        public SimpleReferenceFormatter(Func<string, Table> findTable)
        {
            _findTable = findTable;
        }

        public string FormatColumnClause(SimpleReference reference)
        {
            var formatted = TryFormatAsObjectReference(reference as ObjectReference)
                            ??
                            TryFormatAsFunctionReference(reference as FunctionReference)
                            ??
                            TryFormatAsMathReference(reference as MathReference);

            if (formatted != null) return formatted;

            throw new InvalidOperationException("SimpleReference type not supported.");
        }

        private string FormatObject(object value)
        {
            var reference = value as SimpleReference;
            if (reference != null)
                return FormatColumnClause(reference);
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

        private string TryFormatAsFunctionReference(FunctionReference functionReference)
        {
            if (ReferenceEquals(functionReference, null)) return null;

            var odataName = _functionNameConverter.ConvertToODataName(functionReference.Name);
            var columnArgument = FormatColumnClause(functionReference.Argument);

            if (functionReference.AdditionalArguments.Any())
            {
                var additionalArguments = FormatAdditionalArguments(functionReference.AdditionalArguments);
                if (odataName == "substringof")
                    return string.Format("{0}({1},{2})", odataName, additionalArguments, columnArgument);
                else
                    return string.Format("{0}({1},{2})", odataName, columnArgument, additionalArguments);
            }
            else
            {
                return string.Format("{0}({1})", odataName, columnArgument);
            }
        }

        private string FormatAdditionalArguments(IEnumerable<object> additionalArguments)
        {
            return string.Join(",", additionalArguments.Select(new ValueFormatter().FormatQueryStringValue));
        }

        private string TryFormatAsObjectReference(ObjectReference objectReference)
        {
            if (ReferenceEquals(objectReference, null)) return null;

            if (_findTable == null)
            {
                return objectReference.GetName();
            }
            else
            {
                var objectNames = objectReference.GetAllObjectNames();
                var pathNames = BuildObjectPath(new List<string>(), null, objectNames.ToList());
                return string.Join("/", pathNames.Skip(1));
            }
        }

        private IEnumerable<string> BuildObjectPath(List<string> pathNames, Table table, List<string> objectNames)
        {
            if (!objectNames.Any())
            {
                return pathNames;
            }
            else if (!pathNames.Any())
            {
                table = _findTable(objectNames.First());
                pathNames.Add(table.ActualName);
                return BuildObjectPath(pathNames, table, objectNames.Skip(1).ToList());
            }
            else if (table == null)
            {
                pathNames.AddRange(objectNames);
                return BuildObjectPath(pathNames, null, new List<string>());
            }
            else
            {
                var objectName = objectNames.First();
                if (table.HasColumn(objectName))
                {
                    pathNames.Add(table.FindColumn(objectName).ActualName);
                    return BuildObjectPath(pathNames, null, objectNames.Skip(1).ToList());
                }
                else
                {
                    var association = table.FindAssociation(objectName);
                    pathNames.Add(association.ActualName);
                    return BuildObjectPath(pathNames, _findTable(association.ReferenceTableName), objectNames.Skip(1).ToList());
                }
            }
        }
    }
}
