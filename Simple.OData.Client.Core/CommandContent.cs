using System.Xml.Linq;

namespace Simple.OData.Client
{
    class CommandContent
    {
        public XElement Entry { get; private set; }

        public CommandContent(XElement entry)
        {
            this.Entry = entry;
        }

        public override string ToString()
        {
            return this.Entry.ToString();
        }
    }
}