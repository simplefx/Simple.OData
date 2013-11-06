using System.Linq;
using Simple.OData.Client;

namespace Simple.Data.OData
{
    public class ExpressionConverter
    {
        private readonly bool _excludeResourceTypeExpressions;

        public ExpressionConverter(bool excludeResourceTypeExpressions)
        {
            _excludeResourceTypeExpressions = excludeResourceTypeExpressions;
        }

        public ODataExpression ConvertExpression(SimpleExpression expression)
        {
            return Convert(expression);
        }

        public string GetReferencedResourceType(SimpleExpression expression)
        {
            return FindResourceType(expression);
        }

        private ODataExpression Convert(object value)
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

        private ODataExpression Convert(SimpleExpression expression)
        {
            switch (expression.Type)
            {
                case SimpleExpressionType.And:
                    if (IsResourceTypeExpression(expression.LeftOperand))
                        return Convert(expression.RightOperand);
                    else if (IsResourceTypeExpression(expression.RightOperand))
                        return Convert(expression.LeftOperand);
                    else
                        return Convert(expression.LeftOperand) && Convert(expression.RightOperand);
                case SimpleExpressionType.Or:
                    if (IsResourceTypeExpression(expression.LeftOperand))
                        return Convert(expression.RightOperand);
                    else if (IsResourceTypeExpression(expression.RightOperand))
                        return Convert(expression.LeftOperand);
                    else
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

        private ODataExpression Convert(SimpleReference reference)
        {
            var formattedReference = reference.GetAliasOrName();
            if (reference is ObjectReference)
            {
                formattedReference = string.Join(".", (reference as ObjectReference).GetAllObjectNames().Skip(1));
            }
            return ODataFilter.ExpressionFromReference(formattedReference);
        }

        private ODataExpression Convert(FunctionReference function)
        {
            return ODataFilter.ExpressionFromFunction(function.Name,
                                                 function.Argument.GetAliasOrName(),
                                                 function.AdditionalArguments);
        }

        private string FindResourceType(object value)
        {
            if (value is SimpleExpression)
                return FindResourceType(value as SimpleExpression);
            else
                return null;
        }

        private string FindResourceType(SimpleExpression expression)
        {
            string resourceType = null;
            switch (expression.Type)
            {
                case SimpleExpressionType.And:
                case SimpleExpressionType.Or:
                    resourceType = FindResourceType(expression.LeftOperand);
                    resourceType = resourceType ?? FindResourceType(expression.RightOperand);
                    break;
                case SimpleExpressionType.Equal:
                    if (expression.LeftOperand is ObjectReference &&
                        (expression.LeftOperand as ObjectReference).GetAliasOrName() == ODataFeed.ResourceTypeLiteral)
                        resourceType = expression.RightOperand.ToString();
                    break;
            }
            return resourceType;
        }

        private bool IsResourceTypeExpression(object value)
        {
            if (!_excludeResourceTypeExpressions)
                return false;

            var expression = value as SimpleExpression;
            if (expression != null && expression.Type == SimpleExpressionType.Equal)
            {
                if (expression.LeftOperand is ObjectReference &&
                    (expression.LeftOperand as ObjectReference).GetAliasOrName() == ODataFeed.ResourceTypeLiteral)
                {
                    return true;
                }
            }
            return false;
        }
    }
}