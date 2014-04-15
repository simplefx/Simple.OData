namespace Simple.OData.Client.Tests
{
    public static class Properties
    {
        public static class XmlSamples
        {
            public static string XmlWithDefaultNamespace
            {
                get { return ResourceLoader.LoadFileAsStringAsync("Resources", "XmlWithDefaultNamespace.txt").Result; }
            }

            public static string XmlWithNoNamespace
            {
                get { return ResourceLoader.LoadFileAsStringAsync("Resources", "XmlWithNoNamespace.txt").Result; }
            }

            public static string XmlWithPrefixedNamespace
            {
                get { return ResourceLoader.LoadFileAsStringAsync("Resources", "XmlWithPrefixedNamespace.txt").Result; }
            }

            public static string TwitterStatusesSample
            {
                get { return ResourceLoader.LoadFileAsStringAsync("Resources", "TwitterStatusesSample.txt").Result; }
            }
        }
    }
}