using minuit2.net;
using minuit2.net.CostFunctions;

namespace minuit2.UnitTests.MinimizationProblems;

public record PreconfiguredProblem(
    ICostFunction Cost,
    IReadOnlyCollection<double> OptimumParameterValues,
    IReadOnlyCollection<ParameterConfiguration> ParameterConfigurations)
{
    public double InitialCostValue()
    {
        var orderedParameterValues = ParameterConfigurations
            .OrderBy(p => Cost.Parameters.IndexOf(p.Name))
            .Select(p => p.Value)
            .ToArray();
        
        return Cost.ValueFor(orderedParameterValues);
    }

    public static PreconfiguredProblem QuadraticPolynomialLeastSquares()
    {
        var problem = new QuadraticPolynomialLeastSquaresProblem();
        return new PreconfiguredProblem(
            problem.Cost.Build(),
            problem.OptimumParameterValues,
            problem.ParameterConfigurations.WithAnyValuesCloseToOptimumValues(maximumRelativeBias: 0.1).Build());
    }

    public static PreconfiguredProblem CubicPolynomialLeastSquares()
    {
        var problem = new CubicPolynomialLeastSquaresProblem();
        return new PreconfiguredProblem(
            problem.Cost.Build(),
            problem.OptimumParameterValues,
            problem.ParameterConfigurations.WithAnyValuesCloseToOptimumValues(maximumRelativeBias: 0.1).Build());
    }

    public static PreconfiguredProblem ExponentialDecayLeastSquares()
    {
        var problem = new ExponentialDecayLeastSquaresProblem();
        return new PreconfiguredProblem(
            problem.Cost.Build(),
            problem.OptimumParameterValues,
            problem.ParameterConfigurations.WithAnyValuesCloseToOptimumValues(maximumRelativeBias: 0.1).Build());
    }
    
    public static PreconfiguredProblem BellCurveLeastSquares()
    {
        var problem = new BellCurveLeastSquaresProblem();
        return new PreconfiguredProblem(
            problem.Cost.Build(),
            problem.OptimumParameterValues,
            problem.ParameterConfigurations.WithAnyValuesCloseToOptimumValues(maximumRelativeBias: 0.1).Build());
    }
}