using System;
using System.Collections.Generic;
using System.Linq;
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

                case ExpressionType.Call:
                    return ParseCallExpression(expression);

                case ExpressionType.Constant:
                    return new FilterExpression((expression as ConstantExpression).Value);

                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.Negate:
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
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                    return ParseBinaryExpression(expression);
            }

            throw new NotSupportedException(string.Format("Not supported expression of type {0} ({1}): {2}",
                expression.GetType(), expression.NodeType, expression));
        }

        private static FilterExpression ParseMemberExpression(Expression expression)
        {
            var memberExpression = expression as MemberExpression;
            if (memberExpression.Expression == null)
            {
                return new FilterExpression(EvaluateStaticMember(memberExpression));
            }
            else
            {
                FunctionMapping mapping;
                if (memberExpression.Expression.NodeType == ExpressionType.MemberAccess &&
                    FunctionMapping.SupportedFunctions.TryGetValue(new ExpressionFunction.FunctionCall(memberExpression.Member.Name, 0), out mapping))
                {
                    var contextExpression = memberExpression.Expression as MemberExpression;
                    return FromFunction(memberExpression.Member.Name, contextExpression.Member.Name, new List<object>());
                }
                else
                {
                    return new FilterExpression(memberExpression.Member.Name);
                }
            }
        }

        private static FilterExpression ParseCallExpression(Expression expression)
        {
            var callExpression = expression as MethodCallExpression;

            var memberExpression = callExpression.Object as MemberExpression;
            if (memberExpression == null)
                throw new NotSupportedException(string.Format("Not supported object expression of type {0} in method {1}: {2}",
                    memberExpression.NodeType, callExpression.Method.Name, memberExpression));

            if (callExpression.Arguments.Any(x => x.NodeType != ExpressionType.Constant))
                throw new NotSupportedException(string.Format("Not supported arguments in method {0}",
                    callExpression.Method.Name));
            var arguments = new List<object>();
            arguments.AddRange(callExpression.Arguments.Select(x => (x as ConstantExpression).Value));

            return FromFunction(callExpression.Method.Name, memberExpression.Member.Name, arguments);
        }

        private static FilterExpression ParseUnaryExpression(Expression expression)
        {
            var unaryExpression = (expression as UnaryExpression).Operand;
            var filterExpression = ParseLinqExpression(unaryExpression);
            switch (expression.NodeType)
            {
                case ExpressionType.Not:
                    return !filterExpression;
                case ExpressionType.Convert:
                    return filterExpression;
                case ExpressionType.Negate:
                    return -filterExpression;
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
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return leftExpression + rightExpression;
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return leftExpression - rightExpression;
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return leftExpression * rightExpression;
                case ExpressionType.Divide:
                    return leftExpression / rightExpression;
                case ExpressionType.Modulo:
                    return leftExpression % rightExpression;
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