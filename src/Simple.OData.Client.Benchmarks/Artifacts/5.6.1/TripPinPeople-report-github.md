``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 10.0.17763
Intel Core i7-4790 CPU 3.60GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=2.1.502
  [Host]     : .NET Core 2.0.9 (CoreCLR 4.6.26614.01, CoreFX 4.6.26614.01), 64bit RyuJIT
  DefaultJob : .NET Core 2.0.9 (CoreCLR 4.6.26614.01, CoreFX 4.6.26614.01), 64bit RyuJIT


```
|                               Method |       Mean |     Error |    StdDev |     Median |
|------------------------------------- |-----------:|----------:|----------:|-----------:|
|   FindTypedPeopleWithTripsAndFriends | 8,668.0 us | 172.27 us | 191.47 us | 8,652.4 us |
| FindUntypedPeopleWithTripsAndFriends | 7,510.5 us | 143.00 us | 175.61 us | 7,491.5 us |
|            ConvertWithNewtonsoftJson |   907.3 us |  18.69 us |  39.02 us |   890.5 us |
