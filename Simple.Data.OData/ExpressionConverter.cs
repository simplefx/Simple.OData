using System.Linq;
using Simple.OData.Client;

namespace Simple.Data.OData
{
    public class ExpressionConverter
    {
        public string ConvertExpression(SimpleExpression expression)
        {
            return Convert(expression);
        }

        private FilterExpression Convert(object value)
        {
            if (value is SimpleExpression)
                return Convert(value as SimpleExpression);
            else if (value is FunctionReference)
                return Convert(value as FunctionReference);
            else if (value is SimpleReference)
                return Convert(value as SimpleReference);
            else
                return ODataFilter.ExpressionFromValue(value);
        }

        private FilterExpression Convert(SimpleExpression expression)
        {
            switch (expression.Type)
            {
                case SimpleExpressionType.And:
                    return Convert(expression.LeftOperand) && Convert(expression.RightOperand);
                case SimpleExpressionType.Or:
                    return Convert(expression.LeftOperand) || Convert(expression.RightOperand);
                case SimpleExpressionType.Equal:
                    return Convert(expression.LeftOperand) == Convert(expression.RightOperand);
                case SimpleExpressionType.NotEqual:
                    return Convert(expression.LeftOperand) != Convert(expression.RightOperand);
                case SimpleExpressionType.GreaterThan:
                    return Convert(expression.LeftOperand) > Convert(expression.RightOperand);
                case SimpleExpressionType.GreaterThanOrEqual:
                    return Convert(expression.LeftOperand) >= Convert(expression.RightOperand);
                case SimpleExpressionType.LessThan:
                    return Convert(expression.LeftOperand) < Convert(expression.RightOperand);
                case SimpleExpressionType.LessThanOrEqual:
                    return Convert(expression.LeftOperand) <= Convert(expression.RightOperand);
                case SimpleExpressionType.Function:
                    return Convert(expression.LeftOperand);
                default:
                    return null;
            }
        }

        private FilterExpression Convert(SimpleReference reference)
        {
            var formattedReference = reference.GetAliasOrName();
            if (reference is ObjectReference)
            {
                formattedReference = string.Join(".", (reference as ObjectReference).GetAllObjectNames().Skip(1));
            }
            return ODataFilter.ExpressionFromReference(formattedReference);
        }

        private FilterExpression Convert(FunctionReference function)
        {
            return ODataFilter.ExpressionFromFunction(function.Name,
                                                 function.Argument.GetAliasOrName(),
                                                 function.AdditionalArguments);
        }
    }
}