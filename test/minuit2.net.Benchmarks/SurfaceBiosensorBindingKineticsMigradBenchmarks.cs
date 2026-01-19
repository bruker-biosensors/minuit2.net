using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using ExampleProblems;
using ExampleProblems.CustomProblems;
using static ExampleProblems.DerivativeConfiguration;
using static minuit2.net.Strategy;

namespace minuit2.net.Benchmarks;

[SuppressMessage("ReSharper", "UnassignedField.Global", Justification = "[Params] property is set at runtime by Benchmark.NET")]
[Orderer(SummaryOrderPolicy.Method)]
public class SurfaceBiosensorBindingKineticsMigradBenchmarks
{
    [Params(WithoutDerivatives, WithGradient)]
    // Benchmark all configurations once Hessian-indexing bug is fixed in Minuit2
    // (see https://github.com/root-project/root/pull/20936)
    public DerivativeConfiguration DerivativeConfiguration;  

    [Params(Fast, Balanced, Rigorous, VeryRigorous)]
    public Strategy Strategy;

    [Benchmark]
    public IMinimizationResult SingleBindingKinetics() =>
        new SurfaceBiosensorBindingKineticsProblem(DerivativeConfiguration).MinimizeWithMigrad(Strategy);

    [Benchmark]
    public IMinimizationResult GlobalBindingKinetics()
    {
        var analyteConcentrations = Values.LogarithmicallySpacedBetween(1, 100, 50);
        var problem = SurfaceBiosensorBindingKineticsProblem.Global(analyteConcentrations, DerivativeConfiguration);
        return problem.MinimizeWithMigrad(Strategy);
    }
}