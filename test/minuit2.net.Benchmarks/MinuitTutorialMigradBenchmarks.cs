using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using ExampleProblems.MinuitTutorialProblems;
using minuit2.net.Minimizers;

namespace minuit2.net.Benchmarks;

[SuppressMessage("ReSharper", "UnassignedField.Global", Justification = "[ParamsSource] property is set at runtime by Benchmark.NET.")]
public class MinuitTutorialMigradBenchmarks
{
    private readonly IMinimizer _minimizer = Minimizer.Migrad;
    
    [ParamsSource(typeof(AnalyticalDerivativeConfiguration), nameof(AnalyticalDerivativeConfiguration.All))]
    public AnalyticalDerivativeConfiguration? Config;

    [Benchmark]
    public IMinimizationResult RosenbrockProblem()
    {
        var (hasGradient, hasHessian, hasHessianDiagonal) = Config!;
        var problem = new RosenbrockProblem(hasGradient, hasHessian, hasHessianDiagonal);
        return _minimizer.Minimize(problem.Cost, problem.ParameterConfigurations);
    }
    
    [Benchmark]
    public IMinimizationResult WoodProblem()
    {
        var (hasGradient, hasHessian, hasHessianDiagonal) = Config!;
        var problem = new WoodProblem(hasGradient, hasHessian, hasHessianDiagonal);
        return _minimizer.Minimize(problem.Cost, problem.ParameterConfigurations);
    }
}