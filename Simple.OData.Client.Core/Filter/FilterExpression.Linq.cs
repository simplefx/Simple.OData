using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Simple.OData.Client
{
    public partial class FilterExpression
    {
        private static FilterExpression ParseLinqExpression(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return ParseMemberExpression(expression);

                case ExpressionType.Constant:
                    return new FilterExpression((expression as ConstantExpression).Value);

                case ExpressionType.Not:
                case ExpressionType.Convert:
                    return ParseUnaryExpression(expression);

                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return ParseBinaryExpression(expression);
            }

            throw new NotSupportedException(string.Format("Not supported expression of type {0} ({1}): {2}", 
                expression.GetType(), expression.NodeType, expression));
        }

        private static FilterExpression ParseMemberExpression(Expression expression)
        {
            var memberExpression = expression as MemberExpression;
            if (memberExpression.Expression == null)
                return new FilterExpression(EvaluateStaticMember(memberExpression));
            else
                return new FilterExpression(memberExpression.Member.Name);
        }

        private static FilterExpression ParseUnaryExpression(Expression expression)
        {
            var filterExpression = ParseLinqExpression(expression as UnaryExpression);
            switch (expression.NodeType)
            {
                case ExpressionType.Not:
                    return !filterExpression;
                case ExpressionType.Convert:
                    return filterExpression;
            }

            throw new NotSupportedException(string.Format("Not supported expression of type {0} ({1}): {2}",
                expression.GetType(), expression.NodeType, expression));
        }

        private static FilterExpression ParseBinaryExpression(Expression expression)
        {
            var binaryExpression = expression as BinaryExpression;
            var leftExpression = ParseLinqExpression(binaryExpression.Left);
            var rightExpression = ParseLinqExpression(binaryExpression.Right);

            switch (expression.NodeType)
            {
                case ExpressionType.Equal:
                    return leftExpression == rightExpression;
                case ExpressionType.NotEqual:
                    return leftExpression != rightExpression;
                case ExpressionType.LessThan:
                    return leftExpression < rightExpression;
                case ExpressionType.LessThanOrEqual:
                    return leftExpression <= rightExpression;
                case ExpressionType.GreaterThan:
                    return leftExpression > rightExpression;
                case ExpressionType.GreaterThanOrEqual:
                    return leftExpression >= rightExpression;
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return leftExpression && rightExpression;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return leftExpression || rightExpression;
            }

            throw new NotSupportedException(string.Format("Not supported expression of type {0} ({1}): {2}",
                expression.GetType(), expression.NodeType, expression));
        }

        private static object EvaluateStaticMember(MemberExpression expression)
        {
            object value = null;
            if (expression.Member is FieldInfo)
            {
                var fi = (FieldInfo)expression.Member;
                value = fi.GetValue(null);
            }
            else if (expression.Member is PropertyInfo)
            {
                var pi = (PropertyInfo)expression.Member;
                if (pi.GetIndexParameters().Length != 0)
                    throw new ArgumentException("cannot eliminate closure references to indexed properties");
                value = pi.GetValue(null, null);
            }
            return value;
        }
    }
}