using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using ExampleProblems;
using ExampleProblems.NISTProblems;
using minuit2.net.Minimizers;
using static ExampleProblems.DerivativeConfiguration;
using static minuit2.net.Strategy;

namespace minuit2.net.Benchmarks;

[SuppressMessage("ReSharper", "UnassignedField.Global", Justification = "[Params] property is set at runtime by Benchmark.NET.")]
[Orderer(SummaryOrderPolicy.Method)]
public class NistProblemsMigradBenchmarks
{
    [Params(WithoutDerivatives, WithGradient, WithGradientAndHessian, WithGradientHessianAndHessianDiagonal)]
    public DerivativeConfiguration DerivativeConfiguration;

    [Params(Fast, Balanced, Rigorous, VeryRigorous)]
    public Strategy Strategy;

    private static IMinimizationResult Minimize(IConfiguredProblem problem, Strategy strategy)
    {
        var minimizerConfiguration = new MinimizerConfiguration(strategy);
        return Minimizer.Migrad.Minimize(problem.Cost, problem.ParameterConfigurations, minimizerConfiguration);
    }

    [Benchmark]
    public IMinimizationResult Mgh09Problem() => Minimize(new Mgh09Problem(DerivativeConfiguration), Strategy);

    [Benchmark]
    public IMinimizationResult ThurberProblem() => Minimize(new ThurberProblem(DerivativeConfiguration), Strategy);

    [Benchmark]
    public IMinimizationResult Rat43Problem() => Minimize(new Rat43Problem(DerivativeConfiguration), Strategy);
}