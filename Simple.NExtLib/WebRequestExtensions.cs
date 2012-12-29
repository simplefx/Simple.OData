using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Simple.NExtLib
{
#if NETFX_CORE
    public static class WebRequestExtensions
    {
        public static void SetContent(this WebRequest request, string content)
        {
            var restult = SetContentAsync(request, content).Result;
        }

        public static async Task<int> SetContentAsync(this WebRequest request, string content)
        {
            using (var stream = await request.GetRequestStreamAsync())
            {
                var encoding = new UTF8Encoding();
                var bytes = encoding.GetBytes(content);
                await stream.WriteAsync(bytes, 0, bytes.Length);
                return content.Length;
            }
        }
    }
#else
    public static class WebRequestExtensions
    {
        public static void SetContent(this WebRequest request, string content)
        {
            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(content);
            }
        }
    }
#endif
}
