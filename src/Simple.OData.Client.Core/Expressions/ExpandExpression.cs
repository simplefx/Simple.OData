using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Simple.OData.Client
{
    internal static class ExpandExpression
    {
        public static IEnumerable<ODataExpandAssociation> ExtractExpandAssociations<T>(this Expression<Func<T, object>> expression,
            ITypeCache typeCache)
        {
            var lambdaExpression = expression as LambdaExpression;
            if (lambdaExpression == null)
                throw Utils.NotSupportedExpression(expression);

            switch (lambdaExpression.Body.NodeType)
            {
                case ExpressionType.MemberAccess:
                case ExpressionType.Convert:
                    return ExtractExpandAssociations(lambdaExpression.Body, typeCache);

                case ExpressionType.Call:
                    return ExtractExpandAssociations(lambdaExpression.Body as MethodCallExpression, typeCache);

                case ExpressionType.New:
                    return ExtractExpandAssociations(lambdaExpression.Body as NewExpression, typeCache);

                default:
                    throw Utils.NotSupportedExpression(lambdaExpression.Body);
            }
        }

        private static IEnumerable<ODataExpandAssociation> ExtractExpandAssociations(Expression expression, ITypeCache typeCache)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return ExtractExpandAssociations(expression as MemberExpression, typeCache);

                case ExpressionType.Convert:
                    return ExtractExpandAssociations((expression as UnaryExpression).Operand, typeCache);

                case ExpressionType.Lambda:
                    return ExtractExpandAssociations((expression as LambdaExpression).Body, typeCache);

                case ExpressionType.Call:
                    return ExtractExpandAssociations(expression as MethodCallExpression, typeCache);

                case ExpressionType.New:
                    return ExtractExpandAssociations(expression as NewExpression, typeCache);

                default:
                    throw Utils.NotSupportedExpression(expression);
            }
        }

        private static IEnumerable<ODataExpandAssociation> ExtractExpandAssociations(MethodCallExpression callExpression, ITypeCache typeCache)
        {
            var associations = ExtractExpandAssociations(callExpression.Arguments[0], typeCache);
            if (callExpression.Method.Name == "Select" && callExpression.Arguments.Count == 2)
            {
                associations.Last().ExpandAssociations
                    .AddRange(ExtractExpandAssociations(callExpression.Arguments[1], typeCache));

                return associations;
            }

            if ((callExpression.Method.Name == "OrderBy" ||
                 callExpression.Method.Name == "ThenBy") && callExpression.Arguments.Count == 2)
            {
                if (callExpression.Arguments[0] is MethodCallExpression &&
                    ((callExpression.Arguments[0] as MethodCallExpression).Method.Name == "Select"))
                    throw Utils.NotSupportedExpression(callExpression);

                associations.Last().OrderByColumns
                    .AddRange(callExpression.Arguments[1]
                        .ExtractColumnNames(typeCache).Select(c => new ODataOrderByColumn(c, false)));

                return associations;
            }

            if ((callExpression.Method.Name == "OrderByDescending" ||
                 callExpression.Method.Name == "ThenByDescending") && callExpression.Arguments.Count == 2)
            {
                if (callExpression.Arguments[0] is MethodCallExpression &&
                    ((callExpression.Arguments[0] as MethodCallExpression).Method.Name == "Select"))
                    throw Utils.NotSupportedExpression(callExpression);

                associations.Last().OrderByColumns
                    .AddRange(callExpression.Arguments[1]
                        .ExtractColumnNames(typeCache).Select(c => new ODataOrderByColumn(c, true)));

                return associations;
            }

            if (callExpression.Method.Name == "Where" && callExpression.Arguments.Count == 2)
            {
                var filterExpression =
                    ODataExpression.FromLinqExpression((callExpression.Arguments[1] as LambdaExpression).Body);
                var association = associations.Last();
                if (ReferenceEquals(association.FilterExpression, null))
                    association.FilterExpression = filterExpression;
                else
                    association.FilterExpression = association.FilterExpression && filterExpression;
                return associations;
            }

            throw Utils.NotSupportedExpression(callExpression);
        }

        private static IEnumerable<ODataExpandAssociation> ExtractExpandAssociations(NewExpression newExpression, ITypeCache typeCache)
        {
            return newExpression.Arguments.SelectMany(x => ExtractExpandAssociations(x, typeCache));
        }

        private static IEnumerable<ODataExpandAssociation> ExtractExpandAssociations(MemberExpression memberExpression, ITypeCache typeCache)
        {
            var memberName = typeCache.GetMappedName(memberExpression.Expression.Type, memberExpression.Member.Name);

            return memberExpression.Expression is MemberExpression
                ? ExtractExpandAssociations(memberExpression.Expression, typeCache)
                : new[] { new ODataExpandAssociation(memberName) };
        }
    }
}