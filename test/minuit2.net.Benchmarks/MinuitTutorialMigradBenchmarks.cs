using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using ExampleProblems;
using ExampleProblems.MinuitTutorialProblems;
using static ExampleProblems.DerivativeConfiguration;

namespace minuit2.net.Benchmarks;

[SuppressMessage("ReSharper", "UnassignedField.Global", Justification = "[Params] property is set at runtime by Benchmark.NET")]
[Orderer(SummaryOrderPolicy.Method)]
public class MinuitTutorialMigradBenchmarks
{
    [Params(WithoutDerivatives, WithGradient, WithGradientAndHessian, WithGradientHessianAndHessianDiagonal)]
    public DerivativeConfiguration DerivativeConfiguration;

    [Params(Strategy.Fast, Strategy.Balanced, Strategy.Rigorous, Strategy.VeryRigorous)]
    public Strategy Strategy;

    [Benchmark]
    public IMinimizationResult RosenbrockProblem() =>
        new RosenbrockProblem(DerivativeConfiguration).MinimizeWithMigrad(Strategy);

    [Benchmark]
    public IMinimizationResult WoodProblem() =>
        new WoodProblem(DerivativeConfiguration).MinimizeWithMigrad(Strategy);

    [Benchmark]
    public IMinimizationResult PowellProblem() =>
        new PowellProblem(DerivativeConfiguration).MinimizeWithMigrad(Strategy);

    [Benchmark]
    public IMinimizationResult FletcherPowellProblem() =>
        new FletcherPowellProblem(DerivativeConfiguration).MinimizeWithMigrad(Strategy);

    [Benchmark]
    public IMinimizationResult GoldsteinPriceProblem() =>
        new GoldsteinPriceProblem(DerivativeConfiguration).MinimizeWithMigrad(Strategy);
}