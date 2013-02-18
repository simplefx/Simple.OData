using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.OData.Client.TestUtils
{
    public interface IBinaryTest
    {
        void Run<T>(T expected, T actual);
    }
}
