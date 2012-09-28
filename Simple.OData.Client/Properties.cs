
using Simple.NExtLib;

namespace Simple.OData.Client
{
    public static class Properties
    {
        public static class Resources
        {
            public static string DataServicesAtomEntryXml
            {
                get { return ResourceLoader.LoadFileAsString("Resources", "DataServicesAtomEntryXml.txt"); }
            }

            public static string DataServicesMetadataEntryXml
            {
                get { return ResourceLoader.LoadFileAsString("Resources", "DataServicesMetadataEntryXml.txt"); }
            }
        }
    }
}