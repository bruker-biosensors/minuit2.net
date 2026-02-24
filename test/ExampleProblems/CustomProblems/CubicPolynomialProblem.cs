using minuit2.net;
using static ExampleProblems.DerivativeConfiguration;
using static minuit2.net.ParameterConfiguration;

namespace ExampleProblems.CustomProblems;

public class CubicPolynomialProblem(
    ParameterConfiguration? c0 = null,
    ParameterConfiguration? c1 = null,
    ParameterConfiguration? c2 = null,
    ParameterConfiguration? c3 = null,
    bool hasYErrors = true,
    DerivativeConfiguration derivativeConfiguration = WithoutDerivatives,
    double errorDefinitionInSigma = 1)
    : LeastSquaresProblem(
        XValues,
        YValues,
        hasYErrors ? YError : null,
        [c0 ?? DefaultC0, c1 ?? DefaultC1, c2 ?? DefaultC2, c3 ?? DefaultC3],
        OptimumValues,
        Model,
        ModelGradient,
        ModelHessian,
        ModelHessianDiagonal,
        derivativeConfiguration,
        errorDefinitionInSigma)
{
    private static readonly Func<double, IReadOnlyList<double>, double> Model =
        (x, c) => c[0] + c[1] * x + c[2] * x * x + c[3] * x * x * x;

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelGradient =
        (x, _) => [1, x, x * x, x * x * x];

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelHessian =
        (_, _) => Enumerable.Repeat(0d, 4 * 4).ToArray();

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelHessianDiagonal =
        (_, _) => Enumerable.Repeat(0d, 4).ToArray();

    // The following values are generated using the above model with coefficients c0 = 10, c1 = -2, c2 = 1, c3 = -0.1,
    // adding random normal noise with a standard deviation of 0.1
    private static readonly IReadOnlyList<double> XValues =
    [
        0, 0.5, 1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 5.5, 6, 6.5, 7, 7.5, 8, 8.5, 9, 9.5
    ];

    private static readonly IReadOnlyList<double> YValues =
    [
        9.9, 9.2, 9.03, 8.93, 9.29, 9.75, 10.24, 11.02, 11.57, 12.11, 12.51, 12.46, 12.52, 11.72, 10.8, 9.08, 6.95,
        3.77, 0.07, -4.45
    ];

    private const double YError = 0.1;

    // Since the model is linear in its coefficients, the optimal parameter values are fully determined by the
    // closed-form solution of a linear regression (here, rounded to 2-digit precision)
    private static readonly IReadOnlyList<double> OptimumValues = [9.97, -1.96, 0.99, -0.1];

    // Default parameter configurations
    private static readonly ParameterConfiguration DefaultC0 = Variable("c0", 10.75);
    private static readonly ParameterConfiguration DefaultC1 = Variable("c1", -1.97);
    private static readonly ParameterConfiguration DefaultC2 = Variable("c2", 1.13);
    private static readonly ParameterConfiguration DefaultC3 = Variable("c3", -0.11);
}