using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Simple.OData.Client.Extensions
{
    //public static class WebRequestExtensions
    //{
    //    public static void SetContent(this WebRequest request, string content)
    //    {
    //        using (var stream = request.GetRequestStreamAsync().Result)
    //        {
    //            var encoding = new UTF8Encoding();
    //            var bytes = encoding.GetBytes(content);
    //            stream.Write(bytes, 0, bytes.Length);
    //        }
    //    }

    //    private static Task<Stream> GetRequestStreamAsync(this WebRequest request)
    //    {
    //        return Task.Factory.FromAsync<Stream>(
    //            request.BeginGetRequestStream,
    //            request.EndGetRequestStream,
    //            null);
    //    }
    //}
}
