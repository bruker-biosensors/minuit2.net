using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using ExampleProblems;
using ExampleProblems.MinuitTutorialProblems;
using minuit2.net.Minimizers;
using static ExampleProblems.DerivativeConfiguration;

namespace minuit2.net.Benchmarks;

[SuppressMessage("ReSharper", "UnassignedField.Global", Justification = "[Params] property is set at runtime by Benchmark.NET.")]
[Orderer(SummaryOrderPolicy.Method)]
public class MinuitTutorialMigradBenchmarks
{
    [Params(WithoutDerivatives, WithGradient, WithGradientAndHessian, WithGradientHessianAndHessianDiagonal)]
    public DerivativeConfiguration DerivativeConfiguration;

    [Params(Strategy.Fast, Strategy.Balanced, Strategy.Rigorous, Strategy.VeryRigorous)]
    public Strategy Strategy;

    private static IMinimizationResult Minimize(IConfiguredProblem problem, Strategy strategy)
    {
        var minimizerConfiguration = new MinimizerConfiguration(strategy);
        return Minimizer.Migrad.Minimize(problem.Cost, problem.ParameterConfigurations, minimizerConfiguration);
    }

    [Benchmark]
    public IMinimizationResult RosenbrockProblem() => Minimize(new RosenbrockProblem(DerivativeConfiguration), Strategy);

    [Benchmark]
    public IMinimizationResult WoodProblem() => Minimize(new WoodProblem(DerivativeConfiguration), Strategy);

    [Benchmark]
    public IMinimizationResult PowellProblem() => Minimize(new PowellProblem(DerivativeConfiguration), Strategy);
}