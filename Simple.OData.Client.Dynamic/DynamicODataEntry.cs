using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    public class DynamicODataEntry : ODataEntry, IDynamicMetaObjectProvider
    {
        internal DynamicODataEntry()
        {
        }

        internal DynamicODataEntry(IDictionary<string, object> entry)
            : base(entry)
        {
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