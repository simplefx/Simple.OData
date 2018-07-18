``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 10.0.17134
Intel Core i7-4790 CPU 3.60GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=2.1.300
  [Host]     : .NET Core 2.0.9 (CoreCLR 4.6.26614.01, CoreFX 4.6.26614.01), 64bit RyuJIT
  DefaultJob : .NET Core 2.0.9 (CoreCLR 4.6.26614.01, CoreFX 4.6.26614.01), 64bit RyuJIT


```
|                               Method |     Mean |     Error |    StdDev |
|------------------------------------- |---------:|----------:|----------:|
|   FindTypedPeopleWithTripsAndFriends | 16.31 ms | 0.3204 ms | 0.5939 ms |
| FindUntypedPeopleWithTripsAndFriends | 14.66 ms | 0.2931 ms | 0.4476 ms |
