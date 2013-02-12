using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Simple.OData.Client.Extensions
{
    public static class WebRequestExtensions
    {
#if NET40
        public static void SetContent(this WebRequest request, string content)
        {
            request.ContentLength = content.Length;
            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(content);
            }
        }

        public static Task<WebResponse> GetResponseAsync(this WebRequest request)
        {
            return Task.Factory.StartNew<WebResponse>(() =>
                                                          {
                                                              var t = Task.Factory.FromAsync<WebResponse>(
                                                                  request.BeginGetResponse,
                                                                  request.EndGetResponse,
                                                                  null);

                                                              return t.Result;
                                                          });
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

        public static WebResponse GetResponse(this HttpWebRequest request)
        {
            var responseAsync = request.GetResponseAsync();
            responseAsync.Wait();
            return responseAsync.Result;
        }
#endif
    }
}
