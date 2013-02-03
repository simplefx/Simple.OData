namespace Simple.NExtLib.Tests
{
    public static class Properties
    {
        public static class Resources
        {
            public static string XmlWithDefaultNamespace
            {
                get { return ResourceLoader.LoadFileAsString("Resources", "XmlWithDefaultNamespace.txt"); }
            }

            public static string XmlWithNoNamespace
            {
                get { return ResourceLoader.LoadFileAsString("Resources", "XmlWithNoNamespace.txt"); }
            }

            public static string XmlWithPrefixedNamespace
            {
                get { return ResourceLoader.LoadFileAsString("Resources", "XmlWithPrefixedNamespace.txt"); }
            }

            public static string TwitterStatusesSample
            {
                get { return ResourceLoader.LoadFileAsString("Resources", "TwitterStatusesSample.txt"); }
            }
        }
    }
}