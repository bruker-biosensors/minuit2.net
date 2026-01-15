using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using ExampleProblems;
using ExampleProblems.NISTProblems;
using static ExampleProblems.DerivativeConfiguration;
using static minuit2.net.Strategy;

namespace minuit2.net.Benchmarks;

[SuppressMessage("ReSharper", "UnassignedField.Global", Justification = "[Params] property is set at runtime by Benchmark.NET")]
[Orderer(SummaryOrderPolicy.Method)]
public class NistProblemsMigradBenchmarks
{
    [Params(WithoutDerivatives, WithGradient, WithGradientAndHessian, WithGradientHessianAndHessianDiagonal)]
    public DerivativeConfiguration DerivativeConfiguration;

    [Params(Fast, Balanced, Rigorous, VeryRigorous)]
    public Strategy Strategy;

    [Benchmark]
    public IMinimizationResult Mgh09Problem() => 
        new Mgh09Problem(DerivativeConfiguration).MinimizeWithMigrad(Strategy);

    [Benchmark]
    public IMinimizationResult ThurberProblem() =>
        new ThurberProblem(DerivativeConfiguration).MinimizeWithMigrad(Strategy);
    
    [Benchmark]
    public IMinimizationResult BoxBodProblem() =>
        new BoxBodProblem(DerivativeConfiguration).MinimizeWithMigrad(Strategy);

    [Benchmark]
    public IMinimizationResult Rat42Problem() => 
        new Rat42Problem(DerivativeConfiguration).MinimizeWithMigrad(Strategy);
    
    [Benchmark]
    public IMinimizationResult Rat43Problem() => 
        new Rat43Problem(DerivativeConfiguration).MinimizeWithMigrad(Strategy);
}