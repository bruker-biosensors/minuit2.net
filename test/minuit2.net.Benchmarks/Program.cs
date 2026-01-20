using BenchmarkDotNet.Running;
using minuit2.net.Benchmarks;

BenchmarkRunner.Run<MinuitTutorialProblemsMigradBenchmarks>();
BenchmarkRunner.Run<NistProblemsMigradBenchmarks>();
BenchmarkRunner.Run<SurfaceBiosensorBindingKineticsMigradBenchmarks>();