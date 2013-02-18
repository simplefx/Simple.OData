using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.OData.Client.TestUtils
{
    public interface IEnumerableTest
    {
        void RunTest<T>(T expected, IEnumerable<T> actual);
    }
}
