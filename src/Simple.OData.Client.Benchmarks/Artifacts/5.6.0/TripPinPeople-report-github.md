``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 10.0.17763
Intel Core i7-4790 CPU 3.60GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=2.1.403
  [Host]     : .NET Core 2.0.9 (CoreCLR 4.6.26614.01, CoreFX 4.6.26614.01), 64bit RyuJIT
  DefaultJob : .NET Core 2.0.9 (CoreCLR 4.6.26614.01, CoreFX 4.6.26614.01), 64bit RyuJIT


```
|                               Method |      Mean |     Error |    StdDev |
|------------------------------------- |----------:|----------:|----------:|
|   FindTypedPeopleWithTripsAndFriends | 10.437 ms | 0.2062 ms | 0.4940 ms |
| FindUntypedPeopleWithTripsAndFriends |  9.657 ms | 0.2022 ms | 0.4523 ms |
|            ConvertWithNewtonsoftJson |  1.051 ms | 0.0210 ms | 0.0478 ms |
