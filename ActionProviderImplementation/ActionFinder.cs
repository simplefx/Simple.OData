using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ActionProviderImplementation
{
    public static class ActionFinder
    {
        public static IEnumerable<ActionInfo> GetActionsFromType(Type type)
        {
            var actionInfos = type
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Select(m => new
                {
                    Method = m,
                    Attribute = Attribute.GetCustomAttribute(m, typeof(ActionAttribute)) as ActionAttribute
                })
                .Where(u => u.Attribute != null)
                .Select(u => new ActionInfo(
                    u.Method,
                    u.Attribute.Binding,
                    u.Attribute.AvailabilityMethodName)
                ).ToArray();
            return actionInfos;
        }
    }
}
