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

    public static MinimizationProblem QuadraticPolynomialLeastSquares =>
        new(QuadraticPolynomial.LeastSquaresCost.Build(),
            QuadraticPolynomial.OptimumParameterValues, 
            QuadraticPolynomial.ParameterConfigurations.Defaults);
    
    public static MinimizationProblem CubicPolynomialLeastSquares =>
        new(CubicPolynomial.LeastSquaresCost.Build(),
            CubicPolynomial.OptimumParameterValues, 
            CubicPolynomial.ParameterConfigurations.Defaults);
}