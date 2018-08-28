``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 10.0.17134
Intel Core i7-4790 CPU 3.60GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=2.1.400
  [Host]     : .NET Core 2.0.9 (CoreCLR 4.6.26614.01, CoreFX 4.6.26614.01), 64bit RyuJIT
  DefaultJob : .NET Core 2.0.9 (CoreCLR 4.6.26614.01, CoreFX 4.6.26614.01), 64bit RyuJIT


```
|                               Method |       Mean |     Error |    StdDev |
|------------------------------------- |-----------:|----------:|----------:|
|   FindTypedPeopleWithTripsAndFriends | 9,965.2 us | 174.13 us | 162.88 us |
| FindUntypedPeopleWithTripsAndFriends | 8,272.6 us | 161.01 us | 191.67 us |
|            ConvertWithNewtonsoftJson |   869.2 us |  17.79 us |  22.50 us |
