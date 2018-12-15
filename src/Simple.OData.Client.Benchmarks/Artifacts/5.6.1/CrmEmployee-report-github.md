``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 10.0.17763
Intel Core i7-4790 CPU 3.60GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=2.1.502
  [Host]     : .NET Core 2.0.9 (CoreCLR 4.6.26614.01, CoreFX 4.6.26614.01), 64bit RyuJIT
  DefaultJob : .NET Core 2.0.9 (CoreCLR 4.6.26614.01, CoreFX 4.6.26614.01), 64bit RyuJIT


```
|    Method |     Mean |     Error |    StdDev |
|---------- |---------:|----------:|----------:|
|    GetAll | 17.57 ms | 0.3484 ms | 0.3728 ms |
| GetSingle | 18.08 ms | 0.3536 ms | 0.3631 ms |
