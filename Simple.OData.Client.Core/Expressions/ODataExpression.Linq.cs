using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Simple.OData.Client
{
    public partial class ODataExpression
    {
        private static ODataExpression ParseLinqExpression(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return ParseMemberExpression(expression);

                case ExpressionType.Call:
                    return ParseCallExpression(expression);

                case ExpressionType.Lambda:
                    return ParseLambdaExpression(expression);

                case ExpressionType.Constant:
                    return ParseConstantExpression(expression);

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

                case ExpressionType.New:
                    return ParseNewExpression(expression);
            }

            throw Utils.NotSupportedExpression(expression);
        }

        private static ODataExpression ParseMemberExpression(Expression expression)
        {
            var memberExpression = expression as MemberExpression;
            if (memberExpression.Expression == null)
            {
                return new ODataExpression(EvaluateStaticMember(memberExpression));
            }
            else
            {
                switch (memberExpression.Expression.NodeType)
                {
                    case ExpressionType.Parameter:
                        return new ODataExpression(memberExpression.Member.Name);
                    case ExpressionType.Constant:
                        return ParseConstantExpression(memberExpression.Expression, memberExpression.Member.Name);
                    case ExpressionType.MemberAccess:
                        FunctionMapping mapping;
                        if (FunctionMapping.SupportedFunctions.TryGetValue(new ExpressionFunction.FunctionCall(memberExpression.Member.Name, 0), out mapping))
                        {
                            var contextExpression = memberExpression.Expression as MemberExpression;
                            return FromFunction(memberExpression.Member.Name, contextExpression.Member.Name, new List<object>());
                        }
                        else
                        {
                            return ParseMemberExpression(memberExpression.Expression as MemberExpression);
                        }

                    default:
                        throw Utils.NotSupportedExpression(expression);
                }
            }
        }

        private static ODataExpression ParseCallExpression(Expression expression)
        {
            var callExpression = expression as MethodCallExpression;
            var memberExpression = Utils.CastExpressionWithTypeCheck<MemberExpression>(callExpression.Object);
            if (callExpression.Arguments.Any(x => x.NodeType != ExpressionType.Constant))
                throw new NotSupportedException(string.Format("Not supported arguments in method {0}", callExpression.Method.Name));
            var arguments = new List<object>();
            arguments.AddRange(callExpression.Arguments.Select(x => (x as ConstantExpression).Value));

            return FromFunction(callExpression.Method.Name, memberExpression.Member.Name, arguments);
        }

        private static ODataExpression ParseLambdaExpression(Expression expression)
        {
            var lambdaExpression = expression as LambdaExpression;

            return ParseLinqExpression(lambdaExpression.Body);
        }

        private static ODataExpression ParseConstantExpression(Expression expression, string memberName = null)
        {
            var constExpression = expression as ConstantExpression;

            if (constExpression.Type.IsValueType || constExpression.Type == typeof(string))
            {
                return new ODataExpression(constExpression.Value);
            }
            else if (constExpression.Type.GetProperties().Any(x => x.Name == memberName))
            {
                return new ODataExpression(constExpression.Type.GetProperties().Single(x => x.Name == memberName)
                    .GetValue(constExpression.Value, null));
            }
            else if (constExpression.Type.GetFields().Any(x => x.Name == memberName))
            {
                return new ODataExpression(constExpression.Type.GetFields().Single(x => x.Name == memberName)
                    .GetValue(constExpression.Value));
            }
            else
            {
                throw Utils.NotSupportedExpression(expression);
            }
        }

        private static ODataExpression ParseUnaryExpression(Expression expression)
        {
            var unaryExpression = (expression as UnaryExpression).Operand;
            var odataExpression = ParseLinqExpression(unaryExpression);
            switch (expression.NodeType)
            {
                case ExpressionType.Not:
                    return !odataExpression;
                case ExpressionType.Convert:
                    return odataExpression;
                case ExpressionType.Negate:
                    return -odataExpression;
            }

            throw Utils.NotSupportedExpression(expression);
        }

        private static ODataExpression ParseBinaryExpression(Expression expression)
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

            throw Utils.NotSupportedExpression(expression);
        }

        private static ODataExpression ParseNewExpression(Expression expression)
        {
            var newExpression = expression as NewExpression;
            return FromValue(newExpression.Constructor.Invoke(newExpression.Arguments.Select(x => ParseLinqExpression(x).Value).ToArray()));
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