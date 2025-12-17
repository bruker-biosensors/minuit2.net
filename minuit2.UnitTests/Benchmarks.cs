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
    
    // By default, problem1 shares all of its parameters with problem2 (since they have the same names);
    // All other parameters are unique, meaning non-shared, unless renamed
    private readonly ConfigurableLeastSquaresProblem _problem1 = new QuadraticPolynomialLeastSquaresProblem();
    private readonly ConfigurableLeastSquaresProblem _problem2 = new CubicPolynomialLeastSquaresProblem();
    private readonly ConfigurableLeastSquaresProblem _problem3 = new ExponentialDecayLeastSquaresProblem();
    private readonly ConfigurableLeastSquaresProblem _problem4 = new BellCurveLeastSquaresProblem();

    [Benchmark]
    public void BasicMinimizationProblem()
    {
        var cost = _problem1.Cost.Build();
        var parameterConfigurations = _problem1.ParameterConfigurations.Build();
        _minimizer.Minimize(cost, parameterConfigurations);
    }
    
    [Benchmark]
    public void CompositeMinimizationProblemWithoutAnalyticalCostFunctionDerivatives()
    {
        var cost = CostFunction.Sum(
            _problem1.Cost.Build(),
            _problem2.Cost.Build(), 
            _problem3.Cost.Build(), 
            _problem4.Cost.Build());
        var parameterConfigurations = _problem2.ParameterConfigurations.Build()
            .Concat(_problem3.ParameterConfigurations.Build())
            .Concat(_problem4.ParameterConfigurations.Build())
            .ToArray();
        _minimizer.Minimize(cost, parameterConfigurations);
    }
    
    [Benchmark]
    public void CompositeMinimizationProblemWithAnalyticalFirstOrderCostFunctionDerivatives()
    {
        var cost = CostFunction.Sum(
            _problem1.Cost.WithGradient().Build(),
            _problem2.Cost.WithGradient().Build(), 
            _problem3.Cost.WithGradient().Build(), 
            _problem4.Cost.WithGradient().Build());
        var parameterConfigurations = _problem2.ParameterConfigurations.Build()
            .Concat(_problem3.ParameterConfigurations.Build())
            .Concat(_problem4.ParameterConfigurations.Build())
            .ToArray();
        _minimizer.Minimize(cost, parameterConfigurations);
    }
    
    [Benchmark]
    public void CompositeMinimizationProblemWithAnalyticalSecondOrderCostFunctionDerivatives()
    {
        var cost = CostFunction.Sum(
            _problem1.Cost.WithHessian().Build(),
            _problem2.Cost.WithHessian().Build(), 
            _problem3.Cost.WithHessian().Build(), 
            _problem4.Cost.WithHessian().Build());
        var parameterConfigurations = _problem2.ParameterConfigurations.Build()
            .Concat(_problem3.ParameterConfigurations.Build())
            .Concat(_problem4.ParameterConfigurations.Build())
            .ToArray();
        _minimizer.Minimize(cost, parameterConfigurations);
    }
    
    [Benchmark]
    public void CompositeMinimizationProblemWithAnalyticalSecondOrderCostFunctionDerivativesUsingGaussNewtonApproximation()
    {
        var cost = CostFunction.Sum(
            _problem1.Cost.UsingGaussNewtonApproximation().Build(),
            _problem2.Cost.UsingGaussNewtonApproximation().Build(), 
            _problem3.Cost.UsingGaussNewtonApproximation().Build(), 
            _problem4.Cost.UsingGaussNewtonApproximation().Build());
        var parameterConfigurations = _problem2.ParameterConfigurations.Build()
            .Concat(_problem3.ParameterConfigurations.Build())
            .Concat(_problem4.ParameterConfigurations.Build())
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
