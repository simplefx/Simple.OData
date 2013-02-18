using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.OData.Client.TestUtils
{
    using Xunit;

    public class EqualTest : IBinaryTest
    {
        public void Run<T>(T expected, T actual)
        {
            Assert.Equal(expected, actual);
        }
    }
}
