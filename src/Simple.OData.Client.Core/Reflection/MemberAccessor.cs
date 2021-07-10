using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Concurrent;

namespace Simple.OData.Client
{
    internal static class MemberAccessor

    {
        static readonly ConcurrentDictionary<(Type, Type, MemberInfo), Delegate> getterCache = new ConcurrentDictionary<(Type, Type, MemberInfo), Delegate>();
        static readonly ConcurrentDictionary<(Type, Type, MemberInfo), Delegate> setterCache = new ConcurrentDictionary<(Type, Type, MemberInfo), Delegate>();

        public static Delegate BuildGetterAccessor(Type type, Type returnType, MemberInfo memberInfo)
        {
            var parameter = Expression.Parameter(type);

            var castedParamter =
                type != memberInfo.DeclaringType ?
                Expression.Convert(parameter, memberInfo.DeclaringType) : (Expression)parameter;

            var delegateType = Expression.GetDelegateType(new[] { typeof(object), returnType });
            var body = (Expression)Expression.PropertyOrField(castedParamter, memberInfo.Name);

            if (body.Type != returnType)
                body = Expression.Convert(body, returnType);

            return Expression.Lambda(delegateType, body, parameter).Compile();
        }

        public static Delegate BuildSetterAccessor(Type type, Type valueType, MemberInfo memberInfo)
        {
            var parameter = Expression.Parameter(type);
            var valueParameter = Expression.Parameter(valueType);

            var castedParameter =
                type != memberInfo.DeclaringType ?
                Expression.Convert(parameter, memberInfo.DeclaringType) : (Expression)parameter;

            var memberType = GetMemberType(memberInfo);
            var castedValueParameter =
                valueType != memberType ?
                Expression.Convert(valueParameter, memberType) : (Expression)valueParameter;

            var delegateType = Expression.GetDelegateType(new[] { typeof(object), valueType, typeof(void) });
            return Expression.Lambda(delegateType, 
                Expression.Assign(
                    Expression.PropertyOrField(castedParameter, memberInfo.Name),
                    castedValueParameter),
                    parameter, valueParameter).Compile();
        }

        private static Type GetMemberType(MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo propertyInfo)
                return propertyInfo.PropertyType;
            else if (memberInfo is FieldInfo fieldInfo)
                return fieldInfo.FieldType;
            return null;
        }

        private static bool CanGet(MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo propertyInfo)
                return propertyInfo.CanRead;
            else if (memberInfo is FieldInfo fieldInfo)
                return true;
            else
                return false;
        }

        private static bool CanSet(MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo propertyInfo)
                return propertyInfo.CanWrite;
            else if (memberInfo is FieldInfo fieldInfo)
                return true;
            else
                return false;
        }

        private static MemberInfo GetMemberInfo(Type type, string memberName)
        {
            if (TryGetMemberInfo(type, memberName, out var memberInfo))
                return memberInfo;

            throw new InvalidOperationException($"Property or field {memberName} not found in type {type.FullName}");
        }

        private static void AssertMemberInfoType(MemberInfo memberInfo)
        {
            if (memberInfo.MemberType == MemberTypes.Field || memberInfo.MemberType == MemberTypes.Property)
                return;

            throw new InvalidOperationException($"Member {memberInfo.Name} is of member type {memberInfo.MemberType}. Only property or field members can be access for value.");
        }

        private static bool TryGetMemberInfo(Type type, string memberName, out MemberInfo memberInfo)
        {
            var propertyInfo = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var fieldInfo = (propertyInfo is null) ?
                 type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) : null;

            memberInfo = (MemberInfo)propertyInfo ?? fieldInfo;

            return !(memberInfo is null);
        }

        public static TMember GetValue<TMember>(object instance, MemberInfo memberInfo)
        {
            AssertMemberInfoType(memberInfo);
            if (instance is null) throw new ArgumentNullException(nameof(instance));

            var type = instance.GetType();
            var key = (type, typeof(TMember), memberInfo);
            if (!getterCache.TryGetValue(key, out var accessor))
            {
                accessor = BuildGetterAccessor(typeof(object), typeof(TMember), memberInfo);
                getterCache.TryAdd(key, accessor);
            }

            return ((Func<object, TMember>)accessor)(instance);
        }

        public static TMember GetValue<TMember>(object instance, string memberName)
        {
            if (instance is null) throw new ArgumentNullException(nameof(instance));

            var type = instance.GetType();
            var memberInfo = GetMemberInfo(type, memberName);

            return GetValue<TMember>(instance, memberInfo);
        }

        public static object GetValue(object instance, MemberInfo memberInfo)
            => GetValue<object>(instance, memberInfo);

        public static object GetValue(object instance, string memberName)
            => GetValue<object>(instance, memberName);

        public static bool TryGetValue<TMember>(object instance, MemberInfo memberInfo, out TMember value)
        {
            AssertMemberInfoType(memberInfo);

            value = default;

            if (instance is null) return false;

            var type = instance.GetType();
            if (!(CanGet(memberInfo)))
                return false;

            var key = (type, typeof(TMember), memberInfo);
            if (!getterCache.TryGetValue(key, out var accessor))
            {
                accessor = BuildGetterAccessor(typeof(object), typeof(TMember), memberInfo);
                getterCache.TryAdd(key, accessor);
            }

            try
            {
                value = ((Func<object, TMember>)accessor)(instance);
            }
            catch { return false; }

            return true;
        }

        public static bool TryGetValue<TMember>(object instance, string memberName, out TMember value)
        {
            value = default;

            if (instance is null) return false;

            var type = instance.GetType();
            var memberInfo = GetMemberInfo(type, memberName);

            return TryGetValue<TMember>(instance, memberInfo, out value);
        }

        public static bool TryGetValue(object instance, MemberInfo memberInfo, out object value)
            => TryGetValue<object>(instance, memberInfo, out value);

        public static bool TryGetValue(object instance, string memberName, out object value)
            => TryGetValue<object>(instance, memberName, out value);

        public static void SetValue<TMember>(object instance, MemberInfo memberInfo, TMember value)
        {
            AssertMemberInfoType(memberInfo);

            if (instance is null) throw new ArgumentNullException(nameof(instance));

            var type = instance.GetType();
            var key = (type, typeof(TMember), memberInfo);
            if (!setterCache.TryGetValue(key, out var accessor))
            {
                accessor = BuildSetterAccessor(typeof(object), typeof(TMember), memberInfo);
                setterCache.TryAdd(key, accessor);
            }

            ((Action<object, TMember>)accessor)(instance, value);
        }

        public static void SetValue<TMember>(object instance, string memberName, TMember value)
        {
            if (instance is null) throw new ArgumentNullException(nameof(instance));

            var type = instance.GetType();
            var memberInfo = GetMemberInfo(type, memberName);

            SetValue(instance, memberInfo, value);
        }

        public static void SetValue(object instance, MemberInfo memberInfo, object value)
            => SetValue<object>(instance, memberInfo, value);

        public static void SetValue(object instance, string memberName, object value)
            => SetValue<object>(instance, memberName, value);

        public static bool TrySetValue<TMember>(object instance, MemberInfo memberInfo, TMember value)
        {
            AssertMemberInfoType(memberInfo);

            if (instance is null) return false;

            var type = instance.GetType();
            if (!(CanSet(memberInfo)))
                return false;

            var key = (type, typeof(TMember), memberInfo);
            if (!setterCache.TryGetValue(key, out var accessor))
            {
                accessor = BuildSetterAccessor(typeof(object), typeof(TMember), memberInfo);
                setterCache.TryAdd(key, accessor);
            }

            try
            {
                ((Action<object, TMember>)accessor)(instance, value);
            }
            catch { return false; }

            return true;
        }

        public static bool TrySetValue<TMember>(object instance, string memberName, TMember value)
        {
            if (instance is null) return false;

            var type = instance.GetType();
            var memberInfo = GetMemberInfo(type, memberName);

            return TrySetValue(instance, memberInfo, value);
        }

        public static bool TrySetValue(object instance, MemberInfo memberInfo, object value)
            => TrySetValue<object>(instance, memberInfo, value);

        public static bool TrySetValue(object instance, string memberName, object value)
            => TrySetValue<object>(instance, memberName, value);
    }
}
