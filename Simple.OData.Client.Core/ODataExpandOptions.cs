using System.Diagnostics.Contracts;

namespace Simple.OData.Client
{
    public enum ODataExpandLevels
    {
        Max,
    }
    public enum ODataExpandMode
    {
        ByValue,
        ByReference,
    }

    public class ODataExpandOptions
    {
        public int Levels { get; private set; }
        public ODataExpandMode ExpandMode { get; private set; }

        private ODataExpandOptions(int levels = 1, ODataExpandMode expandMode = ODataExpandMode.ByValue)
        {
            this.Levels = levels;
            this.ExpandMode = expandMode;
        }

        private ODataExpandOptions(ODataExpandLevels levels, ODataExpandMode expandMode = ODataExpandMode.ByValue)
            : this(0, expandMode)
        {
        }

        public static ODataExpandOptions ByValue(int levels = 1)
        {
            return new ODataExpandOptions(levels, ODataExpandMode.ByValue);
        }

        public static ODataExpandOptions ByValue(ODataExpandLevels levels)
        {
            return new ODataExpandOptions(levels, ODataExpandMode.ByValue);
        }

        public static ODataExpandOptions ByReference(int levels = 1)
        {
            return new ODataExpandOptions(levels, ODataExpandMode.ByReference);
        }

        public static ODataExpandOptions ByReference(ODataExpandLevels levels)
        {
            return new ODataExpandOptions(levels, ODataExpandMode.ByReference);
        }
    }
}
