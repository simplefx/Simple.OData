using System;

namespace Simple.OData.Client.Extensions
{
    static class GuidExtensions
    {
        public static string ToODataString(this Guid guid)
        {
            var value = guid.ToString();
            return string.Format(@"guid'{0}'", value);
        }
    }
}