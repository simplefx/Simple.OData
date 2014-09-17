using System;

namespace Simple.OData.Client
{
    [Flags]
    public enum AdapterVersion
    {
        V3 = 0x01,
        V4 = 0x10,

        Any = V3 | V4,
    }
}