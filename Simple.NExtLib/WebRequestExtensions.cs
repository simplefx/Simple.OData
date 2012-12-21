using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;

namespace Simple.NExtLib
{
#if NETFX_CORE
    public static class WebRequestExtensions
    {
        public async static void SetContent(this WebRequest request, string content)
        {
            using (var stream = await request.GetRequestStreamAsync())
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(content);
                }
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
