``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 10.0.17134
Intel Core i7-4790 CPU 3.60GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=2.1.300
  [Host]     : .NET Core 2.0.9 (CoreCLR 4.6.26614.01, CoreFX 4.6.26614.01), 64bit RyuJIT
  DefaultJob : .NET Core 2.0.9 (CoreCLR 4.6.26614.01, CoreFX 4.6.26614.01), 64bit RyuJIT


```
|    Method |     Mean |     Error |    StdDev |
|---------- |---------:|----------:|----------:|
|    GetAll | 27.26 ms | 0.5122 ms | 0.5260 ms |
| GetSingle | 21.30 ms | 0.4120 ms | 0.4744 ms |
