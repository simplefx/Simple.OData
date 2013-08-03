using System;

namespace Simple.OData.Client
{
    public partial class FilterExpression
    {
        public static implicit operator FilterExpression(bool value) { return FilterExpression.FromValue(value); }
        public static implicit operator FilterExpression(byte value) { return FilterExpression.FromValue(value); }
        public static implicit operator FilterExpression(sbyte value) { return FilterExpression.FromValue(value); }
        public static implicit operator FilterExpression(short value) { return FilterExpression.FromValue(value); }
        public static implicit operator FilterExpression(ushort value) { return FilterExpression.FromValue(value); }
        public static implicit operator FilterExpression(int value) { return FilterExpression.FromValue(value); }
        public static implicit operator FilterExpression(uint value) { return FilterExpression.FromValue(value); }
        public static implicit operator FilterExpression(long value) { return FilterExpression.FromValue(value); }
        public static implicit operator FilterExpression(ulong value) { return FilterExpression.FromValue(value); }
        public static implicit operator FilterExpression(float value) { return FilterExpression.FromValue(value); }
        public static implicit operator FilterExpression(double value) { return FilterExpression.FromValue(value); }
        public static implicit operator FilterExpression(decimal value) { return FilterExpression.FromValue(value); }
        public static implicit operator FilterExpression(DateTime value) { return FilterExpression.FromValue(value); }
        public static implicit operator FilterExpression(DateTimeOffset value) { return FilterExpression.FromValue(value); }
        public static implicit operator FilterExpression(TimeSpan value) { return FilterExpression.FromValue(value); }
        public static implicit operator FilterExpression(Guid value) { return FilterExpression.FromValue(value); }
        public static implicit operator FilterExpression(string value) { return FilterExpression.FromValue(value); }

        public static FilterExpression operator !(FilterExpression expr)
        {
            return new FilterExpression(expr, null, ExpressionOperator.NOT);
        }

        public static FilterExpression operator -(FilterExpression expr)
        {
            return new FilterExpression(expr, null, ExpressionOperator.NEG);
        }

        public static FilterExpression operator ==(FilterExpression expr1, FilterExpression expr2)
        {
            return new FilterExpression(expr1, expr2, ExpressionOperator.EQ);
        }

        public static FilterExpression operator !=(FilterExpression expr1, FilterExpression expr2)
        {
            return new FilterExpression(expr1, expr2, ExpressionOperator.NE);
        }

        public static FilterExpression operator &(FilterExpression expr1, FilterExpression expr2)
        {
            return new FilterExpression(expr1, expr2, ExpressionOperator.AND);
        }

        public static FilterExpression operator |(FilterExpression expr1, FilterExpression expr2)
        {
            return new FilterExpression(expr1, expr2, ExpressionOperator.OR);
        }

        public static FilterExpression operator >(FilterExpression expr1, FilterExpression expr2)
        {
            return new FilterExpression(expr1, expr2, ExpressionOperator.GT);
        }

        public static FilterExpression operator <(FilterExpression expr1, FilterExpression expr2)
        {
            return new FilterExpression(expr1, expr2, ExpressionOperator.LT);
        }

        public static FilterExpression operator >=(FilterExpression expr1, FilterExpression expr2)
        {
            return new FilterExpression(expr1, expr2, ExpressionOperator.GE);
        }

        public static FilterExpression operator <=(FilterExpression expr1, FilterExpression expr2)
        {
            return new FilterExpression(expr1, expr2, ExpressionOperator.LE);
        }

        public static FilterExpression operator +(FilterExpression expr1, FilterExpression expr2)
        {
            return new FilterExpression(expr1, expr2, ExpressionOperator.ADD);
        }

        public static FilterExpression operator -(FilterExpression expr1, FilterExpression expr2)
        {
            return new FilterExpression(expr1, expr2, ExpressionOperator.SUB);
        }

        public static FilterExpression operator *(FilterExpression expr1, FilterExpression expr2)
        {
            return new FilterExpression(expr1, expr2, ExpressionOperator.MUL);
        }

        public static FilterExpression operator /(FilterExpression expr1, FilterExpression expr2)
        {
            return new FilterExpression(expr1, expr2, ExpressionOperator.DIV);
        }

        public static FilterExpression operator %(FilterExpression expr1, FilterExpression expr2)
        {
            return new FilterExpression(expr1, expr2, ExpressionOperator.MOD);
        }

        public static bool operator true(FilterExpression expr)
        {
            return false;
        }

        public static bool operator false(FilterExpression expr)
        {
            return false;
        }
    }
}