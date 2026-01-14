using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using ExampleProblems.MinuitTutorialProblems;
using minuit2.net.Minimizers;
using DerivativeConfiguration = (bool hasGradient, bool hasHessian, bool hasHessianDiagonal);

namespace minuit2.net.Benchmarks;

[SuppressMessage("ReSharper", "UnassignedField.Global", Justification = "[ParamsSource] property is set at runtime by Benchmark.NET.")]
[Orderer(SummaryOrderPolicy.Method)]
public class MinuitTutorialMigradBenchmarks
{
    private readonly IMinimizer _minimizer = Minimizer.Migrad;
    
    public static IEnumerable<DerivativeConfiguration> DerivativeConfigurations =>
    [
        new(false, false, false),
        new(true, false, false),
        new(true, true, false),
        new(true, true, true)
    ];
    
    [ParamsSource(nameof(DerivativeConfigurations))]
    public DerivativeConfiguration DerivativeConfiguration;

    [Benchmark]
    public IMinimizationResult RosenbrockProblem()
    {
        var (hasGradient, hasHessian, hasHessianDiagonal) = DerivativeConfiguration;
        var problem = new RosenbrockProblem(hasGradient, hasHessian, hasHessianDiagonal);
        return _minimizer.Minimize(problem.Cost, problem.ParameterConfigurations);
    }
    
    [Benchmark]
    public IMinimizationResult WoodProblem()
    {
        var (hasGradient, hasHessian, hasHessianDiagonal) = DerivativeConfiguration;
        var problem = new WoodProblem(hasGradient, hasHessian, hasHessianDiagonal);
        return _minimizer.Minimize(problem.Cost, problem.ParameterConfigurations);
    }
}