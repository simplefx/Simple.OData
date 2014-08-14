using System;
using System.Collections.Generic;
using System.Reflection;

namespace Simple.OData.Client.Extensions
{
    public static class MemberInfoExtensions
    {
#if NET40 || SILVERLIGHT || PORTABLE_LEGACY
        public static IEnumerable<object> GetCustomAttributes(this MemberInfo member)
        {
            return member.GetCustomAttributes(true);
        }
#else
#endif
    }
}