namespace Simple.NExtLib.Tests
{
    public static class Properties
    {
        public static class Resources
        {
            public static string XmlWithDefaultNamespace
            {
                get { return ResourceLoader.LoadResourceFileAsString("Resources", "XmlWithDefaultNamespace.txt"); }
            }

            public static string XmlWithNoNamespace
            {
                get { return ResourceLoader.LoadResourceFileAsString("Resources", "XmlWithNoNamespace.txt"); }
            }

            public static string XmlWithPrefixedNamespace
            {
                get { return ResourceLoader.LoadResourceFileAsString("Resources", "XmlWithPrefixedNamespace.txt"); }
            }

            public static string TwitterStatusesSample
            {
                get { return ResourceLoader.LoadResourceFileAsString("Resources", "TwitterStatusesSample.txt"); }
            }
        }
    }
}