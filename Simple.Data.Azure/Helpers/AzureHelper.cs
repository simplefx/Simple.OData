using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Globalization;
using System.Security.Cryptography;
using System.IO;

namespace Simple.Data.Azure.Helpers
{
    public class AzureHelper
    {
        private byte[] _sharedKey;

        public string UrlBase { get; set; }
        public string Account { get; set; }

        public string SharedKey
        {
            get { return Convert.ToBase64String(_sharedKey); }
            set { _sharedKey = Convert.FromBase64String(value); }
        }

        private string CreateRequestUrl(string command)
        {
            return (UrlBase ?? string.Format("http://{0}.table.core.windows.net/", Account)) + command;
        }

        public HttpWebRequest CreateTableRequest(string command, string method, string content = null)
        {
            var uri = CreateRequestUrl(command);
            var request = WebRequest.Create(uri);
            request.Method = method;
            request.ContentLength = (content ?? string.Empty).Length;
            request.Headers.Add("x-ms-date", DateTime.UtcNow.ToString("R", CultureInfo.InvariantCulture));

            if (method == "PUT" || method == "DELETE" || method == "MERGE")
            {
                request.Headers.Add("If-Match", "*");
            }

            SignAndAuthorize(request);

            if (content != null)
            {
                AddContent(content, request);
            }

            return (HttpWebRequest)request;
        }

        private static void AddContent(string content, WebRequest request)
        {
            request.Headers.Add("Content-MD5", HashMD5(content));
            request.ContentType = "application/atom+xml";
            request.SetContent(content);
        }

        private void SignAndAuthorize(WebRequest request)
        {
            var resource = request.RequestUri.PathAndQuery;
            if (resource.Contains("?"))
            {
                resource = resource.Substring(0, resource.IndexOf("?"));
            }

            string stringToSign = GetStringToSign(request, resource);

            var hasher = new HMACSHA256(_sharedKey);

            string signedSignature = Convert.ToBase64String(hasher.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));

            string authorizationHeader = string.Format("SharedKeyLite {0}:{1}", Account, signedSignature);

            request.Headers.Add("Authorization", authorizationHeader);
        }

        public static string HashMD5(string source)
        {
            var md5 = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(source));

            return md5.Select(b => b.ToString("x2")).Aggregate((agg, next) => agg + next);
        }

        private string GetStringToSign(WebRequest request, string resource)
        {
            string stringToSign = string.Format("{0}\n/{1}{2}",
                    request.Headers["x-ms-date"],
                    Account,
                    resource
                );
            return stringToSign;
        }
    }
}
