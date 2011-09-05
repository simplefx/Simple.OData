using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Simple.NExtLib
{
    public static class WebRequestExtensions
    {
        public static void SetContent(this WebRequest request, string content)
        {
            using (var writer = new System.IO.StreamWriter(request.GetRequestStream()))
            {
                writer.Write(content);
            }
        }
    }
}
