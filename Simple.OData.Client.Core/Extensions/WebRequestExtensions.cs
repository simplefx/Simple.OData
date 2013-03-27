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

        public static Task SetContentAsync(this WebRequest request, string content)
        {
            return Task.Factory.StartNew(() =>
            {
                request.ContentLength = content.Length;
                var t = Task.Factory.FromAsync<Stream>(
                    request.BeginGetRequestStream,
                    request.EndGetRequestStream,
                    null);

                using (var writer = new StreamWriter(t.Result))
                {
                    writer.Write(content);
                }
            });
        }
#else
        public static void SetContent(this WebRequest request, string content)
        {
            SetContentAsync(request, content).Wait();
        }

        //public static WebResponse GetResponse(this HttpWebRequest request)
        //{
        //    var responseAsync = request.GetResponseAsync();
        //    return responseAsync.Result;
        //}

        public static async Task SetContentAsync(this WebRequest request, string content)
        {
            using (var stream = await request.GetRequestStreamAsync())
            {
                var encoding = new UTF8Encoding();
                var bytes = encoding.GetBytes(content);
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }
        }
#endif

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
    }
}
