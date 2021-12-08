using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Simple.OData.Client
{
    public partial class ODataExpression
    {
        internal static int ArgumentCounter = 0;

        internal string Format(ExpressionContext context)
        {
            if (context.IsQueryOption && _operator != ExpressionType.Default &&
                _operator != ExpressionType.And && _operator != ExpressionType.Equal)
            {
                throw new InvalidOperationException("Invalid custom query option.");
            }

            if (_operator == ExpressionType.Default && !this.IsValueConversion)
            {
                return this.Reference != null ?
                    FormatReference(context) : this.Function != null ?
                    FormatFunction(context) :
                    FormatValue(context);
            }
            else if (this.IsValueConversion)
            {
                var expr = this.Value as ODataExpression;
                if (expr.Reference == null && expr.Function == null && !expr.IsValueConversion)
                {
                    if (expr.Value != null && context.Session.TypeCache.IsEnumType(expr.Value.GetType()))
                    {
                        expr = new ODataExpression(expr.Value);
                    }
                    else if (context.Session.TypeCache.TryConvert(expr.Value, _conversionType, out var result))
                    {
                        expr = new ODataExpression(result);
                    }
                }
                return FormatExpression(expr, context);
            }
            else if (_operator == ExpressionType.Not || _operator == ExpressionType.Negate)
            {
                var left = FormatExpression(_left, context);
                var op = FormatOperator(context);
                if (NeedsGrouping(_left))
				{
					return $"{op} ({left})";
				}
				else
				{
					return $"{op} {left}";
				}
			}
            else
            {
                var left = FormatExpression(_left, context);
                var right = FormatExpression(_right, context);
                var op = FormatOperator(context);

                if (context.IsQueryOption)
                {
                    return $"{left}{op}{right}";
                }
                else
                {
                    if (NeedsGrouping(_left))
					{
						left = $"({left})";
					}

					if (NeedsGrouping(_right))
					{
						right = $"({right})";
					}

					return $"{left} {op} {right}";
                }
            }
        }

        private static string FormatExpression(ODataExpression expr, ExpressionContext context)
        {
            if (expr is null)
            {
                if (!String.IsNullOrEmpty(context.ScopeQualifier))
				{
					return context.ScopeQualifier;
				}

				return "null";
            }
            else
            {
                return expr.Format(context);
            }
        }

        private string FormatReference(ExpressionContext context)
        {
            var elementNames = new List<string>(this.Reference.Split('.', '/'));
            var entityCollection = context.EntityCollection;
            var segmentNames = BuildReferencePath(new List<string>(), entityCollection, elementNames, context);
            return FormatScope(string.Join("/", segmentNames), context);
        }

        private string FormatFunction(ExpressionContext context)
        {
            var adapterVersion = context.Session?.Adapter.AdapterVersion ?? AdapterVersion.Default;
            if (FunctionToOperatorMapping.TryGetOperatorMapping(_functionCaller, Function, adapterVersion, out var operatorMapping))
            {
                return FormatMappedOperator(context, operatorMapping);
            }
            if (FunctionMapping.TryGetFunctionMapping(this.Function.FunctionName, this.Function.Arguments.Count(), adapterVersion, out var functionMapping))
            {
                return FormatMappedFunction(context, functionMapping);
            }
            else if (string.Equals(this.Function.FunctionName, ODataLiteral.Any, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(this.Function.FunctionName, ODataLiteral.All, StringComparison.OrdinalIgnoreCase))
            {
                return FormatAnyAllFunction(context);
            }
            else if (string.Equals(this.Function.FunctionName, ODataLiteral.IsOf, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(this.Function.FunctionName, ODataLiteral.Cast, StringComparison.OrdinalIgnoreCase))
            {
                return FormatIsOfCastFunction(context);
            }
            else if (string.Equals(this.Function.FunctionName, "get_Item", StringComparison.Ordinal) &&
                this.Function.Arguments.Count == 1)
            {
                return FormatArrayIndexFunction(context);
            }
            else if (string.Equals(this.Function.FunctionName, "HasFlag", StringComparison.Ordinal) &&
                this.Function.Arguments.Count == 1)
            {
                return FormatEnumHasFlagFunction(context);
            }
            else if (string.Equals(this.Function.FunctionName, "ToString", StringComparison.Ordinal) &&
                this.Function.Arguments.Count == 0)
            {
                return FormatToStringFunction(context);
            }
            else if (this.Function.Arguments.Count == 1)
            {
                var val = this.Function.Arguments.First();
                if (val.Value != null)
                {
                    var formattedVal = ODataExpression.FromValue(
                        string.Equals(this.Function.FunctionName, "ToBoolean", StringComparison.Ordinal) ? Convert.ToBoolean(val.Value) :
                        string.Equals(this.Function.FunctionName, "ToByte", StringComparison.Ordinal) ? Convert.ToByte(val.Value) :
                        string.Equals(this.Function.FunctionName, "ToChar", StringComparison.Ordinal) ? Convert.ToChar(val.Value) :
                        string.Equals(this.Function.FunctionName, "ToDateTime", StringComparison.Ordinal) ? Convert.ToDateTime(val.Value) :
                        string.Equals(this.Function.FunctionName, "ToDecimal", StringComparison.Ordinal) ? Convert.ToDecimal(val.Value) :
                        string.Equals(this.Function.FunctionName, "ToDouble", StringComparison.Ordinal) ? Convert.ToDouble(val.Value) :
                        string.Equals(this.Function.FunctionName, "ToInt16", StringComparison.Ordinal) ? Convert.ToInt16(val.Value) :
                        string.Equals(this.Function.FunctionName, "ToInt32", StringComparison.Ordinal) ? Convert.ToInt32(val.Value) :
                        string.Equals(this.Function.FunctionName, "ToInt64", StringComparison.Ordinal) ? Convert.ToInt64(val.Value) :
                        string.Equals(this.Function.FunctionName, "ToSByte", StringComparison.Ordinal) ? Convert.ToSByte(val.Value) :
                        string.Equals(this.Function.FunctionName, "ToSingle", StringComparison.Ordinal) ? Convert.ToSingle(val.Value) :
                        string.Equals(this.Function.FunctionName, "ToString", StringComparison.Ordinal) ? Convert.ToString(val.Value) :
                        string.Equals(this.Function.FunctionName, "ToUInt16", StringComparison.Ordinal) ? Convert.ToUInt16(val.Value) :
                        string.Equals(this.Function.FunctionName, "ToUInt32", StringComparison.Ordinal) ? Convert.ToUInt32(val.Value) :
                        string.Equals(this.Function.FunctionName, "ToUInt64", StringComparison.Ordinal) ? (object)Convert.ToUInt64(val.Value)
                        : null);
                    if (formattedVal.Value != null)
					{
						return FormatExpression(formattedVal, context);
					}
				}
            }

            throw new NotSupportedException($"The function {this.Function.FunctionName} is not supported or called with wrong number of arguments");
        }
        
        private string FormatMappedOperator(ExpressionContext context, FunctionToOperatorMapping mapping)
        {
            return mapping.Format(context, _functionCaller, Function.Arguments);
        }
        
        private string FormatMappedFunction(ExpressionContext context, FunctionMapping mapping)
        {
            var mappedFunction = mapping.FunctionMapper(
                this.Function.FunctionName, _functionCaller, this.Function.Arguments).Function;
            var formattedArguments = string.Join(",",
                (IEnumerable<object>)mappedFunction.Arguments.Select(x => FormatExpression(x, context)));

            return $"{mappedFunction.FunctionName}({formattedArguments})";
        }

        private string FormatAnyAllFunction(ExpressionContext context)
        {
            var navigationPath = FormatCallerReference();
            EntityCollection entityCollection;
            if (!context.Session.Metadata.HasNavigationProperty(context.EntityCollection.Name, navigationPath))
            {
                //simple collection property
                entityCollection = context.EntityCollection;
            }
            else
            {
                entityCollection = context.Session.Metadata.NavigateToCollection(context.EntityCollection, navigationPath);
            }

            string formattedArguments;
            if(!this.Function.Arguments.Any() && string.Equals(this.Function.FunctionName, ODataLiteral.Any, StringComparison.OrdinalIgnoreCase))
            {
                formattedArguments = string.Empty;
            }
            else
            {
                var targetQualifier = $"x{(ArgumentCounter >= 0 ? (1 + (ArgumentCounter++) % 9).ToString() : string.Empty)}";
                var expressionContext = new ExpressionContext(context.Session, entityCollection, targetQualifier, context.DynamicPropertiesContainerName);
                formattedArguments = $"{targetQualifier}:{FormatExpression(this.Function.Arguments.First(), expressionContext)}";
            }

            var formattedNavigationPath = context.Session.Adapter.GetCommandFormatter().FormatNavigationPath(context.EntityCollection, navigationPath);
            return FormatScope($"{formattedNavigationPath}/{this.Function.FunctionName.ToLower()}({formattedArguments})", context);
        }

        private string FormatIsOfCastFunction(ExpressionContext context)
        {
            var formattedArguments = string.Empty;
            if (this.Function.Arguments.First() is not null && !this.Function.Arguments.First().IsNull)
            {
                formattedArguments += FormatExpression(this.Function.Arguments.First(), new ExpressionContext(context.Session));
                formattedArguments += ",";
            }
            formattedArguments += FormatExpression(this.Function.Arguments.Last(), new ExpressionContext(context.Session));

            return $"{this.Function.FunctionName.ToLower()}({formattedArguments})";
        }

        private string FormatEnumHasFlagFunction(ExpressionContext context)
        {
            var value = FormatExpression(this.Function.Arguments.First(), new ExpressionContext(context.Session));
            return $"{FormatCallerReference()} has {value}";
        }

        private string FormatArrayIndexFunction(ExpressionContext context)
        {
            var propertyName =
                FormatExpression(this.Function.Arguments.First(), new ExpressionContext(context.Session)).Trim('\'');
            return _functionCaller.Reference == context.DynamicPropertiesContainerName
                ? propertyName
                : $"{FormatCallerReference()}.{propertyName}";
        }

        private string FormatToStringFunction(ExpressionContext context)
        {
            return _functionCaller.Reference != null 
                ? FormatCallerReference() 
                : _functionCaller.FormatValue(context);
        }

        private string FormatValue(ExpressionContext context)
        {
            if (Value is ODataExpression expression)
            {
                return expression.Format(context);
            }
            else if (Value is Type type)
            {
                var typeName = context.Session.Adapter.GetMetadata().GetQualifiedTypeName(type.Name);
                return context.Session.Adapter.GetCommandFormatter().ConvertValueToUriLiteral(typeName, false);
            }
            else
            {
                return context.Session.Adapter.GetCommandFormatter().ConvertValueToUriLiteral(Value, false);
            }
        }

        private string FormatOperator(ExpressionContext context)
        {
			return _operator switch
			{
				ExpressionType.And => context.IsQueryOption ? "&" : "and",
				ExpressionType.Or => "or",
				ExpressionType.Not => "not",
				ExpressionType.Equal => context.IsQueryOption ? "=" : "eq",
				ExpressionType.NotEqual => "ne",
				ExpressionType.GreaterThan => "gt",
				ExpressionType.GreaterThanOrEqual => "ge",
				ExpressionType.LessThan => "lt",
				ExpressionType.LessThanOrEqual => "le",
				ExpressionType.Add => "add",
				ExpressionType.Subtract => "sub",
				ExpressionType.Multiply => "mul",
				ExpressionType.Divide => "div",
				ExpressionType.Modulo => "mod",
				ExpressionType.Negate => "-",
				_ => null,
			};
		}

        private IEnumerable<string> BuildReferencePath(List<string> segmentNames, EntityCollection entityCollection, List<string> elementNames, ExpressionContext context)
        {
            if (!elementNames.Any())
            {
                return segmentNames;
            }

            var objectName = elementNames.First();
            if (entityCollection != null)
            {
                if (context.Session.Metadata.HasStructuralProperty(entityCollection.Name, objectName))
                {
                    var propertyName = context.Session.Metadata.GetStructuralPropertyExactName(
                        entityCollection.Name, objectName);
                    segmentNames.Add(propertyName);
                    return BuildReferencePath(segmentNames, null, elementNames.Skip(1).ToList(), context);
                }
                else if (context.Session.Metadata.HasNavigationProperty(entityCollection.Name, objectName))
                {
                    var propertyName = context.Session.Metadata.GetNavigationPropertyExactName(
                        entityCollection.Name, objectName);
                    var linkName = context.Session.Metadata.GetNavigationPropertyPartnerTypeName(
                        entityCollection.Name, objectName);
                    var linkedEntityCollection = context.Session.Metadata.GetEntityCollection(linkName);
                    segmentNames.Add(propertyName);
                    return BuildReferencePath(segmentNames, linkedEntityCollection, elementNames.Skip(1).ToList(), context);
                }
                else if (IsFunction(objectName, context))
                {
                    var formattedFunction = FormatAsFunction(objectName, context);
                    segmentNames.Add(formattedFunction);
                    return BuildReferencePath(segmentNames, null, elementNames.Skip(1).ToList(), context);
                }
                else if (context.Session.Metadata.IsOpenType(entityCollection.Name))
                {
                    segmentNames.Add(objectName);
                    return BuildReferencePath(segmentNames, null, elementNames.Skip(1).ToList(), context);
                }
                else
                {
                    throw new UnresolvableObjectException(objectName, $"Invalid referenced object [{objectName}]");
                }
            }
            else if (FunctionMapping.ContainsFunction(elementNames.First(), 0))
            {
                var formattedFunction = FormatAsFunction(objectName, context);
                segmentNames.Add(formattedFunction);
                return BuildReferencePath(segmentNames, null, elementNames.Skip(1).ToList(), context);
            }
            else
            {
                segmentNames.AddRange(elementNames);
                return BuildReferencePath(segmentNames, null, new List<string>(), context);
            }
        }

        private bool IsFunction(string objectName, ExpressionContext context)
        {
            var adapterVersion = context.Session?.Adapter.AdapterVersion ?? AdapterVersion.Default;
            return FunctionMapping.TryGetFunctionMapping(objectName, 0, adapterVersion, out _);
        }

        private string FormatAsFunction(string objectName, ExpressionContext context)
        {
            var adapterVersion = context.Session?.Adapter.AdapterVersion ?? AdapterVersion.Default;
            if (FunctionMapping.TryGetFunctionMapping(objectName, 0, adapterVersion, out var mapping))
            {
                var mappedFunction = mapping.FunctionMapper(objectName, _functionCaller, null).Function;
                return $"{mappedFunction.FunctionName}({FormatCallerReference()})";
            }
            else
            {
                return null;
            }
        }

        private string FormatCallerReference()
        {
            return _functionCaller.Reference.Replace(".", "/");
        }

        private int GetPrecedence(ExpressionType op)
        {
			return op switch
			{
				ExpressionType.Not or ExpressionType.Negate => 1,
				ExpressionType.Modulo or ExpressionType.Multiply or ExpressionType.Divide => 2,
				ExpressionType.Add or ExpressionType.Subtract => 3,
				ExpressionType.GreaterThan or ExpressionType.GreaterThanOrEqual or ExpressionType.LessThan or ExpressionType.LessThanOrEqual => 4,
				ExpressionType.Equal or ExpressionType.NotEqual => 5,
				ExpressionType.And => 6,
				ExpressionType.Or => 7,
				_ => 0,
			};
		}

        private bool NeedsGrouping(ODataExpression expr)
        {
            if (_operator == ExpressionType.Default)
			{
				return false;
			}

			if (expr is null)
			{
				return false;
			}

			if (expr._operator == ExpressionType.Default)
			{
				return false;
			}

			var outerPrecedence = GetPrecedence(_operator);
            var innerPrecedence = GetPrecedence(expr._operator);
            return outerPrecedence < innerPrecedence;
        }

        private string FormatScope(string text, ExpressionContext context)
        {
            return string.IsNullOrEmpty(context.ScopeQualifier)
                ? text
                : $"{context.ScopeQualifier}/{text}";
        }
    }
}
