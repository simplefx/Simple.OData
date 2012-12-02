using System.Linq;

namespace Simple.OData.Client
{
    public partial class FilterExpression
    {

        public override string ToString()
        {
            if (_operator == ExpressionOperator.None)
            {
                return _reference != null ?
                    FormatReference() : _function != null ?
                    FormatFunction() :
                    FormatValue();
            }
            else if (_operator == ExpressionOperator.NOT)
            {
                var left = FormatExpression(_left);
                var op = FormatOperator();
                if (NeedsGrouping(_left))
                    return string.Format("{0}({1})", op, left);
                else
                    return string.Format("{0} {1}", op, left);
            }
            else
            {
                var left = FormatExpression(_left);
                var right = FormatExpression(_right);
                var op = FormatOperator();
                if (NeedsGrouping(_left))
                    return string.Format("({0}) {1} {2}", left, op, right);
                else if (NeedsGrouping(_right))
                    return string.Format("{0} {1} ({2})", left, op, right);
                else
                    return string.Format("{0} {1} {2}", left, op, right);
            }
        }

        private static string FormatExpression(FilterExpression expr)
        {
            return expr == null ? "null" : expr.ToString();
        }

        private string FormatReference()
        {
            return _reference;
        }

        private string FormatFunction()
        {
            return string.Format("{0}({1})", _function.Name, string.Join(",", _function.Arguments.Select(FormatExpression)));
        }

        private string FormatValue()
        {
            return _value == null ?
                "null" : _value is string ?
                string.Format("'{0}'", _value) :
                _value is bool ?
                _value.ToString().ToLower() :
                _value.ToString();
        }

        private string FormatOperator()
        {
            switch (_operator)
            {
                case ExpressionOperator.AND:
                    return "and";
                case ExpressionOperator.OR:
                    return "or";
                case ExpressionOperator.NOT:
                    return "not";
                case ExpressionOperator.EQ:
                    return "eq";
                case ExpressionOperator.NE:
                    return "ne";
                case ExpressionOperator.GT:
                    return "gt";
                case ExpressionOperator.GE:
                    return "ge";
                case ExpressionOperator.LT:
                    return "lt";
                case ExpressionOperator.LE:
                    return "le";
                case ExpressionOperator.ADD:
                    return "add";
                case ExpressionOperator.SUB:
                    return "sub";
                case ExpressionOperator.MUL:
                    return "mul";
                case ExpressionOperator.DIV:
                    return "div";
                case ExpressionOperator.MOD:
                    return "mod";
                default:
                    return null;
            }
        }

        private int GetPrecedence(ExpressionOperator op)
        {
            switch (op)
            {
                case ExpressionOperator.NOT:
                    return 1;
                case ExpressionOperator.MOD:
                case ExpressionOperator.MUL:
                case ExpressionOperator.DIV:
                    return 2;
                case ExpressionOperator.ADD:
                case ExpressionOperator.SUB:
                    return 3;
                case ExpressionOperator.GT:
                case ExpressionOperator.GE:
                case ExpressionOperator.LT:
                case ExpressionOperator.LE:
                    return 4;
                case ExpressionOperator.EQ:
                case ExpressionOperator.NE:
                    return 5;
                case ExpressionOperator.AND:
                    return 6;
                case ExpressionOperator.OR:
                    return 7;
                default:
                    return 0;
            }
        }

        private bool NeedsGrouping(FilterExpression expr)
        {
            if (_operator == ExpressionOperator.None)
                return false;
            if (expr == null)
                return false;
            if (expr._operator == ExpressionOperator.None)
                return false;

            int outerPrecedence = GetPrecedence(_operator);
            int innerPrecedence = GetPrecedence(expr._operator);
            return outerPrecedence < innerPrecedence;
        }
    }
}
