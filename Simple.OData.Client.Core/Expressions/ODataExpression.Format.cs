using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.OData.Client
{
    public partial class ODataExpression
    {
        internal string Format(Session session, EntitySet entitySet)
        {
            return this.Format(new ExpressionContext()
                                   {
                                       Session = session, 
                                       EntitySet = entitySet
                                   });
        }

        internal string Format(Session session, string collection)
        {
            return this.Format(new ExpressionContext { Session = session, Collection = collection });
        }

        internal string Format(ExpressionContext context)
        {
            if (_operator == ExpressionOperator.None)
            {
                return this.Reference != null ?
                    FormatReference(context) : this.Function != null ?
                    FormatFunction(context) :
                    FormatValue(context);
            }
            else if (_operator == ExpressionOperator.NOT || _operator == ExpressionOperator.NEG)
            {
                var left = FormatExpression(_left, context);
                var op = FormatOperator(context);
                if (NeedsGrouping(_left))
                    return string.Format("{0}({1})", op, left);
                else
                    return string.Format("{0} {1}", op, left);
            }
            else
            {
                var left = FormatExpression(_left, context);
                var right = FormatExpression(_right, context);
                var op = FormatOperator(context);
                if (NeedsGrouping(_left))
                    return string.Format("({0}) {1} {2}", left, op, right);
                else if (NeedsGrouping(_right))
                    return string.Format("{0} {1} ({2})", left, op, right);
                else
                    return string.Format("{0} {1} {2}", left, op, right);
            }
        }

        private static string FormatExpression(ODataExpression expr, ExpressionContext context)
        {
            if (ReferenceEquals(expr, null))
            {
                return "null";
            }
            else
            {
                return expr.Format(context);
            }
        }

        private string FormatReference(ExpressionContext context)
        {
            var elementNames = new List<string>(this.Reference.Split('.'));
            var pathNames = BuildReferencePath(new List<string>(), context.EntitySet, elementNames, context);
            return string.Join("/", pathNames);
        }

        private string FormatFunction(ExpressionContext context)
        {
            FunctionMapping mapping;
            if (FunctionMapping.SupportedFunctions.TryGetValue(new ExpressionFunction.FunctionCall(this.Function.FunctionName, this.Function.Arguments.Count()), out mapping))
            {
                var mappedFunction = mapping.FunctionMapper(this.Function.FunctionName, _functionCaller.Format(context), this.Function.Arguments).Function;
                return string.Format("{0}({1})", mappedFunction.FunctionName,
                    string.Join(",", (IEnumerable<object>)mappedFunction.Arguments.Select(x => FormatExpression(x, context))));
            }
            else
            {
                throw new NotSupportedException(string.Format("The function {0} is not supported or called with wrong number of arguments", this.Function.FunctionName));
            }
        }

        private string FormatValue(ExpressionContext context)
        {
            return (new ValueFormatter()).FormatExpressionValue(Value, context);
        }

        private string FormatOperator(ExpressionContext context)
        {
            switch (_operator)
            {
                case ExpressionOperator.AND:
                    return "and";
                case ExpressionOperator.OR:
                    return "or";
                case ExpressionOperator.NOT:
                    return "not";
                case ExpressionOperator.EQ:
                    return "eq";
                case ExpressionOperator.NE:
                    return "ne";
                case ExpressionOperator.GT:
                    return "gt";
                case ExpressionOperator.GE:
                    return "ge";
                case ExpressionOperator.LT:
                    return "lt";
                case ExpressionOperator.LE:
                    return "le";
                case ExpressionOperator.ADD:
                    return "add";
                case ExpressionOperator.SUB:
                    return "sub";
                case ExpressionOperator.MUL:
                    return "mul";
                case ExpressionOperator.DIV:
                    return "div";
                case ExpressionOperator.MOD:
                    return "mod";
                case ExpressionOperator.NEG:
                    return "-";
                default:
                    return null;
            }
        }

        private IEnumerable<string> BuildReferencePath(List<string> pathNames, EntitySet entitySet, List<string> elementNames, ExpressionContext context)
        {
            if (!elementNames.Any())
            {
                return pathNames;
            }

            var objectName = elementNames.First();
            if (entitySet != null)
            {
                if (entitySet.Metadata.HasStructuralProperty(entitySet.ActualName, objectName))
                {
                    pathNames.Add(entitySet.Metadata.GetStructuralPropertyExactName(entitySet.ActualName, objectName));
                    return BuildReferencePath(pathNames, null, elementNames.Skip(1).ToList(), context);
                }
                else if (entitySet.Metadata.HasNavigationProperty(entitySet.ActualName, objectName))
                {
                    pathNames.Add(entitySet.Metadata.GetNavigationPropertyExactName(entitySet.ActualName, objectName));
                    return BuildReferencePath(pathNames, context.Session.MetadataCache.FindEntitySet(
                        entitySet.Metadata.GetNavigationPropertyPartnerName(entitySet.ActualName, objectName)), 
                        elementNames.Skip(1).ToList(), context);
                }
                else
                {
                    var formattedFunction = FormatAsFunction(objectName, context);
                    if (!string.IsNullOrEmpty(formattedFunction))
                    {
                        pathNames.Add(formattedFunction);
                        return BuildReferencePath(pathNames, null, elementNames.Skip(1).ToList(), context);
                    }
                    else
                    {
                        throw new UnresolvableObjectException(objectName, string.Format("Invalid referenced object {0}", objectName));
                    }
                }
            }
            else if (FunctionMapping.SupportedFunctions.ContainsKey(new ExpressionFunction.FunctionCall(elementNames.First(), 0)))
            {
                var formattedFunction = FormatAsFunction(objectName, context);
                pathNames.Add(formattedFunction);
                return BuildReferencePath(pathNames, null, elementNames.Skip(1).ToList(), context);
            }
            else
            {
                pathNames.AddRange(elementNames);
                return BuildReferencePath(pathNames, null, new List<string>(), context);
            }
        }

        private string FormatAsFunction(string objectName, ExpressionContext context)
        {
            FunctionMapping mapping;
            if (FunctionMapping.SupportedFunctions.TryGetValue(new ExpressionFunction.FunctionCall(objectName, 0), out mapping))
            {
                string targetName = _functionCaller.Format(context);
                var mappedFunction = mapping.FunctionMapper(objectName, targetName, null).Function;
                return string.Format("{0}({1})", mappedFunction.FunctionName, targetName);
            }
            else
            {
                return null;
            }
        }

        private int GetPrecedence(ExpressionOperator op)
        {
            switch (op)
            {
                case ExpressionOperator.NOT:
                case ExpressionOperator.NEG:
                    return 1;
                case ExpressionOperator.MOD:
                case ExpressionOperator.MUL:
                case ExpressionOperator.DIV:
                    return 2;
                case ExpressionOperator.ADD:
                case ExpressionOperator.SUB:
                    return 3;
                case ExpressionOperator.GT:
                case ExpressionOperator.GE:
                case ExpressionOperator.LT:
                case ExpressionOperator.LE:
                    return 4;
                case ExpressionOperator.EQ:
                case ExpressionOperator.NE:
                    return 5;
                case ExpressionOperator.AND:
                    return 6;
                case ExpressionOperator.OR:
                    return 7;
                default:
                    return 0;
            }
        }

        private bool NeedsGrouping(ODataExpression expr)
        {
            if (_operator == ExpressionOperator.None)
                return false;
            if (ReferenceEquals(expr, null))
                return false;
            if (expr._operator == ExpressionOperator.None)
                return false;

            int outerPrecedence = GetPrecedence(_operator);
            int innerPrecedence = GetPrecedence(expr._operator);
            return outerPrecedence < innerPrecedence;
        }
    }
}
