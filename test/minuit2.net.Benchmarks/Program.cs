using BenchmarkDotNet.Running;
using minuit2.net.Benchmarks;

BenchmarkRunner.Run<Benchmarks>();
BenchmarkRunner.Run<MinuitTutorialMigradBenchmarks>();
BenchmarkRunner.Run<SurfaceBiosensorBindingKineticsMigradBenchmarks>();