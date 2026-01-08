using BenchmarkDotNet.Attributes;
using ExampleProblems;
using minuit2.net.CostFunctions;
using minuit2.net.Minimizers;

namespace minuit2.net.Benchmarks;

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
    public IMinimizationResult BasicMinimizationProblem()
    {
        var cost = _problem1.Cost.Build();
        var parameterConfigurations = _problem1.ParameterConfigurations.Build();
        return _minimizer.Minimize(cost, parameterConfigurations);
    }

    [Benchmark]
    public IMinimizationResult CompositeMinimizationProblemWithoutAnalyticalCostFunctionDerivatives()
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
        return _minimizer.Minimize(cost, parameterConfigurations);
    }

    [Benchmark]
    public IMinimizationResult CompositeMinimizationProblemWithAnalyticalFirstOrderCostFunctionDerivatives()
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
        return _minimizer.Minimize(cost, parameterConfigurations);
    }

    [Benchmark]
    public IMinimizationResult CompositeMinimizationProblemWithAnalyticalSecondOrderCostFunctionDerivatives()
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
        return _minimizer.Minimize(cost, parameterConfigurations);
    }

    [Benchmark]
    public IMinimizationResult CompositeMinimizationProblemWithAnalyticalSecondOrderCostFunctionDerivativesUsingGaussNewtonApproximation()
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
        return _minimizer.Minimize(cost, parameterConfigurations);
    }
}