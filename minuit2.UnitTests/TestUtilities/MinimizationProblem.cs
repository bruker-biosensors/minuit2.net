using minuit2.net;
using minuit2.net.CostFunctions;

namespace minuit2.UnitTests.TestUtilities;

public record MinimizationProblem(
    ICostFunction Cost,
    IReadOnlyCollection<ParameterConfiguration> ParameterConfigurations,
    IReadOnlyCollection<double> OptimumParameterValues)
{
    public static MinimizationProblem CubicPolynomialLeastSquares =>
        new(CubicPolynomial.LeastSquaresCost.Build(),
            CubicPolynomial.ParameterConfigurations.Defaults,
            CubicPolynomial.OptimumParameterValues);
}