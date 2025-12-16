using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using minuit2.net.CostFunctions;
using minuit2.net.Minimizers;
using minuit2.UnitTests.MinimizationProblems;

namespace minuit2.UnitTests;

[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Benchmarks.NET only supports instance methods.")]
[SuppressMessage("Structure", "NUnit1028:The non-test method is public", Justification = "Benchmarks.NET only supports public methods.")]
public class Benchmarks
{
    private readonly IMinimizer _minimizer = Minimizer.Migrad;
    
    [Benchmark]
    public void SimpleMinimizationProblem()
    {
        var problem = new QuadraticPolynomialLeastSquaresProblem();
        var cost = problem.Cost.Build();
        var parameterConfigurations = problem.ParameterConfigurations.Build();
        
        _minimizer.Minimize(cost, parameterConfigurations);
    }
    
    [Benchmark]
    public void MoreComplexMinimizationProblem()
    {
        // problem1 shares all of its parameters with problem2; all other parameters are unique, meaning non-shared
        var problem1 = new QuadraticPolynomialLeastSquaresProblem();
        var problem2 = new CubicPolynomialLeastSquaresProblem();
        var problem3 = new ExponentialDecayLeastSquaresProblem();
        var problem4 = new BellCurveLeastSquaresProblem();

        var cost = CostFunction.Sum(
            problem1.Cost.Build(),
            problem2.Cost.Build(), 
            problem3.Cost.Build(), 
            problem4.Cost.Build());
        var parameterConfigurations = problem2.ParameterConfigurations.Build()
            .Concat(problem3.ParameterConfigurations.Build())
            .Concat(problem4.ParameterConfigurations.Build())
            .ToArray();
        
        _minimizer.Minimize(cost, parameterConfigurations);
    }

    [Test, Explicit]
    public void Run_benchmarks()
    {
        var config = DefaultConfig.Instance.WithOptions(ConfigOptions.DisableOptimizationsValidator);
        BenchmarkRunner.Run<Benchmarks>(config);
    }
}
