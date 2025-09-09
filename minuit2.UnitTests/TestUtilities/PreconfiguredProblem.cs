using minuit2.net;
using minuit2.net.CostFunctions;

namespace minuit2.UnitTests.TestUtilities;

public record PreconfiguredProblem(
    ICostFunction Cost,
    IReadOnlyCollection<double> OptimumParameterValues,
    IReadOnlyCollection<ParameterConfiguration> ParameterConfigurations)
{
    public double InitialCostValue => Cost.ValueFor(OrderedParameterConfigurations.Select(p => p.Value).ToArray());

    private IOrderedEnumerable<ParameterConfiguration> OrderedParameterConfigurations =>
        ParameterConfigurations.OrderBy(p => Cost.Parameters.IndexOf(p.Name));

    public static PreconfiguredProblem QuadraticPolynomialLeastSquares()
    {
        var problem = new QuadraticPolynomialLeastSquaresProblem();
        return new PreconfiguredProblem(problem.Cost.Build(),
            problem.OptimumParameterValues,
            problem.ParameterConfigurations.WithAnyValuesCloseToOptimumValues(maximumRelativeBias: 0.1).Build());
    }

    public static PreconfiguredProblem CubicPolynomialLeastSquares()
    {
        var problem = new CubicPolynomialLeastSquaresProblem();
        return new PreconfiguredProblem(problem.Cost.Build(),
            problem.OptimumParameterValues,
            problem.ParameterConfigurations.WithAnyValuesCloseToOptimumValues(maximumRelativeBias: 0.1).Build());
    }
}