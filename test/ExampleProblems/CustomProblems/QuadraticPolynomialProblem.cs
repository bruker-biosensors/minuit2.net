using minuit2.net;
using static ExampleProblems.DerivativeConfiguration;
using static minuit2.net.ParameterConfiguration;

namespace ExampleProblems.CustomProblems;

public class QuadraticPolynomialProblem(
    ParameterConfiguration? c0 = null,
    ParameterConfiguration? c1 = null,
    ParameterConfiguration? c2 = null,
    DerivativeConfiguration derivativeConfiguration = WithoutDerivatives, 
    double errorDefinitionInSigma = 1)
    : LeastSquaresProblem(
        XValues,
        YValues,
        YError,
        [c0 ?? DefaultC0, c1 ?? DefaultC1, c2 ?? DefaultC2],
        OptimumValues,
        Model,
        ModelGradient,
        ModelHessian,
        ModelHessianDiagonal,
        derivativeConfiguration,
        errorDefinitionInSigma)
{
    private static readonly Func<double, IReadOnlyList<double>, double> Model =
        (x, c) => c[0] + c[1] * x + c[2] * x * x;

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelGradient =
        (x, _) => [1, x, x * x];

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelHessian =
        (_, _) => Enumerable.Repeat(0d, 3 * 3).ToArray();
    
    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelHessianDiagonal =
        (_, _) => Enumerable.Repeat(0d, 3).ToArray();
    
    // The following values are generated using the above model with coefficients c0 = 10, c1 = -5, c2 = 0.5,
    // adding random normal noise with a standard deviation of 0.1
    private static readonly IReadOnlyList<double> XValues =
    [
        0, 0.5, 1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 5.5, 6, 6.5, 7, 7.5, 8, 8.5, 9, 9.5
    ];
    
    private static readonly IReadOnlyList<double> YValues = 
    [
        9.9, 7.59, 5.63, 3.64, 2.09, 0.68, -0.56, -1.32, -2.03, -2.41, -2.49, -2.53, -1.88, -1.44, -0.4, 0.64, 2.15,
        3.56, 5.47, 7.66
    ];

    private const double YError = 0.1;
    
    // Since the model is linear in its coefficients, the optimal parameter values are fully determined by the
    // closed-form solution of a linear regression (here, rounded to 2-digit precision)
    private static readonly IReadOnlyList<double> OptimumValues = [10, -5, 0.5];
    
    // Default parameter configurations
    public static readonly ParameterConfiguration DefaultC0 = Variable("c0", 10.9);
    public static readonly ParameterConfiguration DefaultC1 = Variable("c1", -6.06);
    public static readonly ParameterConfiguration DefaultC2 = Variable("c2", 0.58);
}