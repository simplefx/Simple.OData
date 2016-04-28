using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    internal static class ColumnExpression
    {
        public static IEnumerable<string> ExtractColumnNames<T>(Expression<Func<T, object>> expression)
        {
            var lambdaExpression = expression as LambdaExpression;
            if (lambdaExpression == null)
                throw Utils.NotSupportedExpression(expression);

            switch (lambdaExpression.Body.NodeType)
            {
                case ExpressionType.MemberAccess:
                case ExpressionType.Convert:
                    return ExtractColumnNames(lambdaExpression.Body);

                case ExpressionType.Call:
                    return ExtractColumnNames(lambdaExpression.Body as MethodCallExpression);

                case ExpressionType.New:
                    return ExtractColumnNames(lambdaExpression.Body as NewExpression);

                default:
                    throw Utils.NotSupportedExpression(lambdaExpression.Body);
            }
        }

        public static string ExtractColumnName(Expression expression)
        {
            return ExtractColumnNames(expression).Single();
        }

        private static IEnumerable<string> ExtractColumnNames(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return ExtractColumnNames(expression as MemberExpression);

                case ExpressionType.Convert:
                    return ExtractColumnNames((expression as UnaryExpression).Operand);

                case ExpressionType.Lambda:
                    return ExtractColumnNames((expression as LambdaExpression).Body);

                case ExpressionType.Call:
                    return ExtractColumnNames(expression as MethodCallExpression);

                case ExpressionType.New:
                    return ExtractColumnNames(expression as NewExpression);

                default:
                    throw Utils.NotSupportedExpression(expression);
            }
        }

        private static IEnumerable<string> ExtractColumnNames(MethodCallExpression callExpression)
        {
            if (callExpression.Method.Name == "Select" && callExpression.Arguments.Count == 2)
            {
                return ExtractColumnNames(callExpression.Arguments[0])
                    .SelectMany(x => ExtractColumnNames(callExpression.Arguments[1])
                        .Select(y => String.Join("/", x, y)));
            }
            else if (callExpression.Method.Name == "OrderBy" && callExpression.Arguments.Count == 2)
            {
                if (callExpression.Arguments[0] is MethodCallExpression && ((callExpression.Arguments[0] as MethodCallExpression).Method.Name == "Select"))
                    throw Utils.NotSupportedExpression(callExpression);

                return ExtractColumnNames(callExpression.Arguments[0])
                    .SelectMany(x => ExtractColumnNames(callExpression.Arguments[1])
                        .OrderBy(y => String.Join("/", x, y)));
            }
            else if (callExpression.Method.Name == "OrderByDescending" && callExpression.Arguments.Count == 2)
            {
                if (callExpression.Arguments[0] is MethodCallExpression && ((callExpression.Arguments[0] as MethodCallExpression).Method.Name == "Select"))
                    throw Utils.NotSupportedExpression(callExpression);

                return ExtractColumnNames(callExpression.Arguments[0])
                    .SelectMany(x => ExtractColumnNames(callExpression.Arguments[1])
                        .OrderByDescending(y => String.Join("/", x, y)));
            }
            else
            {
                throw Utils.NotSupportedExpression(callExpression);
            }
        }

        private static IEnumerable<string> ExtractColumnNames(NewExpression newExpression)
        {
            return newExpression.Arguments.SelectMany(ExtractColumnNames);
        }

        private static IEnumerable<string> ExtractColumnNames(MemberExpression memberExpression)
        {
            var memberName = memberExpression.Expression.Type
                .GetAnyProperty(memberExpression.Member.Name)
                .GetMappedName();

            return memberExpression.Expression is MemberExpression
                ? ExtractColumnNames(memberExpression.Expression)
                    .Select(x => String.Join("/", x, memberName))
                : new[] { memberName };
        }
    }
}