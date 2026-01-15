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
    private readonly IMinimizer _minimizer = Minimizer.Migrad;
    
    [Params(WithoutDerivatives, WithGradient, WithGradientAndHessian, WithGradientHessianAndHessianDiagonal)]
    public DerivativeConfiguration DerivativeConfiguration;

    [Benchmark]
    public IMinimizationResult RosenbrockProblem()
    {
        var problem = new RosenbrockProblem(DerivativeConfiguration);
        return _minimizer.Minimize(problem.Cost, problem.ParameterConfigurations);
    }
    
    [Benchmark]
    public IMinimizationResult WoodProblem()
    {
        var problem = new WoodProblem(DerivativeConfiguration);
        return _minimizer.Minimize(problem.Cost, problem.ParameterConfigurations);
    }
    
    [Benchmark]
    public IMinimizationResult PowellProblem()
    {
        var problem = new PowellProblem(DerivativeConfiguration);
        return _minimizer.Minimize(problem.Cost, problem.ParameterConfigurations);
    }
}