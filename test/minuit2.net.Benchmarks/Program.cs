using BenchmarkDotNet.Running;
using minuit2.net.Benchmarks;

BenchmarkRunner.Run<Benchmarks>();
BenchmarkRunner.Run<MinuitTutorialProblemsMigradBenchmarks>();
BenchmarkRunner.Run<SurfaceBiosensorBindingKineticsMigradBenchmarks>();