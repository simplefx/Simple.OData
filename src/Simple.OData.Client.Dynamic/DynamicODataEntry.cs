using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using Simple.OData.Client.Extensions;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public class DynamicODataEntry : ODataEntry, IDynamicMetaObjectProvider
    {
        internal DynamicODataEntry()
        {
        }

        internal DynamicODataEntry(IDictionary<string, object> entry)
            : base(ToDynamicODataEntry(entry))
        {
        }

        private static IDictionary<string, object> ToDynamicODataEntry(IDictionary<string, object> entry)
        {
            return entry == null
                ? null
                : entry.ToDictionary(
                        x => x.Key,
                        y => y.Value is IDictionary<string, object>
                            ? new DynamicODataEntry(y.Value as IDictionary<string, object>)
                            : y.Value is IEnumerable<object>
                            ? ToDynamicODataEntry(y.Value as IEnumerable<object>)
                            : y.Value);
        }

        private static IEnumerable<object> ToDynamicODataEntry(IEnumerable<object> entry)
        {
            return entry == null
                ? null
                : entry.Select(x => x is IDictionary<string, object>
                    ? new DynamicODataEntry(x as IDictionary<string, object>)
                    : x).ToList();
        }

        private object GetEntryValue(string propertyName)
        {
            var value = base[propertyName];
            if (value is IDictionary<string, object>)
                value = new DynamicODataEntry(value as IDictionary<string, object>);
            return value;
        }

        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new DynamicEntryMetaObject(parameter, this);
        }

        private class DynamicEntryMetaObject : DynamicMetaObject
        {
            internal DynamicEntryMetaObject(
                Expression parameter,
                DynamicODataEntry value)
                : base(parameter, BindingRestrictions.Empty, value)
            {
            }

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
            {
                var methodInfo = typeof(DynamicODataEntry).GetDeclaredMethod("GetEntryValue");
                var arguments = new Expression[]
                {
                    Expression.Constant(binder.Name)
                };

                return new DynamicMetaObject(
                    Expression.Call(Expression.Convert(Expression, LimitType), methodInfo, arguments),
                    BindingRestrictions.GetTypeRestriction(Expression, LimitType));
            }

            public override DynamicMetaObject BindConvert(ConvertBinder binder)
            {
                var value = this.HasValue
                    ? (this.Value as ODataEntry).AsDictionary().ToObject(binder.Type)
                    : null;

                return new DynamicMetaObject(
                    Expression.Constant(value),
                    BindingRestrictions.GetTypeRestriction(Expression, LimitType));
            }
        }
    }
}