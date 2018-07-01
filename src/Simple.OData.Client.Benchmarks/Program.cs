using System;
using System.Threading;
using BenchmarkDotNet.Running;

namespace Simple.OData.Client.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<CrmEmployee>();
        }
    }
}
