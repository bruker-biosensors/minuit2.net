using minuit2.net;
using minuit2.net.CostFunctions;

namespace minuit2.UnitTests.TestUtilities;

public record MinimizationProblem(
    ICostFunction Cost,
    IReadOnlyCollection<double> OptimumParameterValues,
    IReadOnlyCollection<ParameterConfiguration> ParameterConfigurations)
{
    public double InitialCostValue => Cost.ValueFor(OrderedParameterConfigurations.Select(p => p.Value).ToArray());

    private IOrderedEnumerable<ParameterConfiguration> OrderedParameterConfigurations =>
        ParameterConfigurations.OrderBy(p => Cost.Parameters.IndexOf(p.Name));

    public static MinimizationProblem QuadraticPolynomialLeastSquares()
    {
        var problem = new QuadraticPolynomialLeastSquaresProblem();
        return new MinimizationProblem(problem.Cost.Build(),
            problem.OptimumParameterValues,
            problem.ParameterConfigurations.WithAnyValuesCloseToOptimumValues(maximumRelativeBias: 0.1).Build());
    }

    public static MinimizationProblem CubicPolynomialLeastSquares =>
        new(CubicPolynomial.LeastSquaresCost.Build(),
            CubicPolynomial.OptimumParameterValues, 
            CubicPolynomial.ParameterConfigurations.Defaults);
}