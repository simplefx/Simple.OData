using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Simple.NExtLib
{
    public static class WebRequestExtensions
    {
#if (NET20 || NET35 || NET40)
        public static void SetContent(this WebRequest request, string content)
        {
            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(content);
            }
        }
#else
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

        internal static Task<WebResponse> GetResponseAsync(this WebRequest request, TimeSpan timeout)
        {
            return Task.Factory.StartNew<WebResponse>(() =>
                                                          {
                                                              var t = Task.Factory.FromAsync<WebResponse>(
                                                                  request.BeginGetResponse,
                                                                  request.EndGetResponse,
                                                                  null);

                                                              if (!t.Wait(timeout)) throw new TimeoutException();

                                                              return t.Result;
                                                          });
        }
#endif
    }
}
