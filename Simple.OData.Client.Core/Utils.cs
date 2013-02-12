using System.IO;
using System.Text;

namespace Simple.OData.Client
{
    public class Utils
    {
        public static string StreamToString(Stream stream)
        {
            string result;

            using (var reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }

            return result;
        }

        public static Stream StringToStream(string str)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(str));
        }
    }
}
