
namespace Simple.OData.Client
{
    public static class Properties
    {
        private const string _dataServicesAtomEntryXml =
            @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>" + 
            @"<entry xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns=""http://www.w3.org/2005/Atom"">" + 
            @"  <title />" + 
            @"  <updated/>" + 
            @"  <author>" + 
            @"    <name />" + 
            @"  </author>" + 
            @"  <id />" + 
            @"  <category scheme=""http://schemas.microsoft.com/ado/2007/08/dataservices/scheme""/>" +
            @"  <content type=""application/xml"">" + 
            @"    <m:properties/>" + 
            @"  </content>" + 
            @"  <link rel=""http://schemas.microsoft.com/ado/2007/08/dataservices/related/"" type=""application/atom+xml;type=Entry"" />" + 
            @"</entry>";

        private const string _dataServicesMetadataEntryXml = 
            @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>" + 
            @"<uri xmlns=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"">" + 
            @"</uri>";

        public static class Resources
        {
            public static string DataServicesAtomEntryXml
            {
                get { return _dataServicesAtomEntryXml; }
            }

            public static string DataServicesMetadataEntryXml
            {
                get { return _dataServicesMetadataEntryXml; }
            }
        }
    }
}