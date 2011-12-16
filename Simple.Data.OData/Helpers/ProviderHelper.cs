using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Globalization;
using System.Security.Cryptography;
using System.IO;

namespace Simple.Data.OData.Helpers
{
    using Simple.NExtLib;

    public class ProviderHelper
    {
        public string UrlBase { get; set; }

        private string CreateRequestUrl(string command)
        {
            return (UrlBase ?? "http://") + command;
        }

        public HttpWebRequest CreateTableRequest(string command, string method, string content = null)
        {
            var uri = CreateRequestUrl(command);
            var request = WebRequest.Create(uri);
            request.Method = method;
            request.ContentLength = (content ?? string.Empty).Length;

            // TODO: revise
            //if (method == "PUT" || method == "DELETE" || method == "MERGE")
            //{
            //    request.Headers.Add("If-Match", "*");
            //}

            if (content != null)
            {
                AddContent(content, request);
            }

            return (HttpWebRequest)request;
        }

        private static void AddContent(string content, WebRequest request)
        {
            request.ContentType = "application/atom+xml";
            request.SetContent(content);
        }
    }
}
