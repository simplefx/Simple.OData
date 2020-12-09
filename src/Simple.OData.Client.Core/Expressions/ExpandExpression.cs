using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Simple.OData.Client
{
    internal static class ExpandExpression
    {
        public static IEnumerable<ODataExpandAssociation> ExtractExpandAssociations<T>(
            this Expression<Func<T, object>> expression,
            ITypeCache typeCache)
        {
            var expandExpressionVisitor = new ExpandExpressionVisitor(typeCache);
            expandExpressionVisitor.Visit(expression);

            if (expandExpressionVisitor.ExpandAssociations.Any())
                return expandExpressionVisitor.ExpandAssociations;

            throw Utils.NotSupportedExpression(expression);
        }
    }

    internal class ExpandExpressionVisitor : ExpressionVisitor
    {
        private readonly ITypeCache _typeCache;
        
        public ExpandExpressionVisitor(ITypeCache typeCache)
        {
            _typeCache = typeCache;
        }

        public List<ODataExpandAssociation> ExpandAssociations { get; } = new List<ODataExpandAssociation>();

        protected override Expression VisitMember(MemberExpression node)
        {
            var currentMemberExpression = node;
            var memberName = _typeCache.GetMappedName(currentMemberExpression.Expression.Type, currentMemberExpression.Member.Name);
            var expandAssociation = new ODataExpandAssociation(memberName);
            while (currentMemberExpression.Expression is MemberExpression memberExpression)
            {
                currentMemberExpression = memberExpression;
                memberName = _typeCache.GetMappedName(currentMemberExpression.Expression.Type, currentMemberExpression.Member.Name);
                expandAssociation = new ODataExpandAssociation(memberName)
                {
                    ExpandAssociations = { expandAssociation }
                };
            }
            ExpandAssociations.Add(expandAssociation);
            
            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType != ExpressionType.Convert) return base.VisitUnary(node);
            
            ExpandAssociations.AddRange(ExtractNestedExpandAssociations(node.Operand));
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Arguments.Count != 2)
                throw Utils.NotSupportedExpression(node);

            var association = ExtractNestedExpandAssociations(node.Arguments[0]).Single();
            ExpandAssociations.Add(association);
            var deepestChildAssociation = association;
            while (deepestChildAssociation.ExpandAssociations.Any())
            {
                deepestChildAssociation = deepestChildAssociation.ExpandAssociations.First();
            }

            switch (node.Method.Name)
            {
                case "Select":
                {
                    deepestChildAssociation.ExpandAssociations.AddRange(ExtractNestedExpandAssociations(node.Arguments[1]));

                    return node;
                }
                case "OrderBy":
                case "ThenBy":
                {
                    if ((node.Arguments[0] as MethodCallExpression)?.Method.Name == "Select")
                        throw Utils.NotSupportedExpression(node);

                    deepestChildAssociation.OrderByColumns
                        .AddRange(node.Arguments[1]
                            .ExtractColumnNames(_typeCache).Select(c => new ODataOrderByColumn(c, false)));

                    return node;
                }
                case "OrderByDescending":
                case "ThenByDescending":
                {
                    if ((node.Arguments[0] as MethodCallExpression)?.Method.Name == "Select")
                        throw Utils.NotSupportedExpression(node);

                    deepestChildAssociation.OrderByColumns
                        .AddRange(node.Arguments[1]
                            .ExtractColumnNames(_typeCache).Select(c => new ODataOrderByColumn(c, true)));

                    return node;
                }
                case "Where":
                {
                    var filterExpression =
                        ODataExpression.FromLinqExpression((node.Arguments[1] as LambdaExpression)?.Body);
                    if (ReferenceEquals(association.FilterExpression, null))
                        deepestChildAssociation.FilterExpression = filterExpression;
                    else
                        deepestChildAssociation.FilterExpression = deepestChildAssociation.FilterExpression && filterExpression;

                    return node;
                }
                default:
                    throw Utils.NotSupportedExpression(node);
            }
        }

        protected override Expression VisitNew(NewExpression node)
        {
            ExpandAssociations.AddRange(node.Arguments.SelectMany(ExtractNestedExpandAssociations));
            return node;
        }

        private IEnumerable<ODataExpandAssociation> ExtractNestedExpandAssociations(Expression expression)
        {
            var nestedExpandExpressionVisitor = new ExpandExpressionVisitor(_typeCache);
            nestedExpandExpressionVisitor.Visit(expression);
            return nestedExpandExpressionVisitor.ExpandAssociations;
        }
    }
}