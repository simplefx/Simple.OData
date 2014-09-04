using System.Xml.Linq;

namespace Simple.OData.Client
{
    class CommandContent
    {
        public string Entry { get; private set; }

        public CommandContent(XElement entry)
        {
            this.Entry = entry.ToString();
        }

        public CommandContent(string entry)
        {
            this.Entry = entry;
        }

        public override string ToString()
        {
            return this.Entry.ToString();
        }
    }
}