using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
                var odataEntry = base.Value as DynamicODataEntry;
                if (odataEntry.AsDictionary().ContainsKey(binder.Name))
                {
                    var value = odataEntry.AsDictionary()[binder.Name];
                    if (value is IDictionary<string, object>)
                        value = new DynamicODataEntry(value as IDictionary<string, object>);
                    Expression objectExpression = Expression.Constant(value);
                    if (value != null && value.GetType().IsValueType)
                    {
                        objectExpression = Expression.Convert(objectExpression, typeof(object));
                    }

                    return new DynamicMetaObject(
                        objectExpression, 
                        BindingRestrictions.GetTypeRestriction(Expression, LimitType));
                }
                return base.BindGetMember(binder);
            }
        }
    }
}