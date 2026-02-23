using minuit2.net;
using static ExampleProblems.DerivativeConfiguration;
using static minuit2.net.ParameterConfiguration;

namespace ExampleProblems.CustomProblems;

public class ExponentialDecayProblem(
    ParameterConfiguration? amplitude = null, 
    ParameterConfiguration? rate = null,
    ParameterConfiguration? offset = null,
    DerivativeConfiguration derivativeConfiguration = WithoutDerivatives) 
    : LeastSquaresProblem(
        XValues,
        YValues,
        YError,
        [amplitude ?? DefaultAmplitude, rate ?? DefaultRate, offset ?? DefaultOffset],
        OptimumValues,
        Model,
        ModelGradient,
        ModelHessian,
        ModelHessianDiagonal,
        derivativeConfiguration)
{
    private static readonly Func<double, IReadOnlyList<double>, double> Model = 
        (x, p) => p[0] * Math.Exp(-p[1] * x) + p[2];

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelGradient = 
        (x, p) =>
        [
            Math.Exp(-p[1] * x),
            -p[0] * x * Math.Exp(-p[1] * x),
            1
        ];

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelHessian = 
        (x, p) =>
        {
            var h01 = -x * Math.Exp(-p[1] * x);
            var h11 = p[0] * x * x * Math.Exp(-p[1] * x);
            return [0, h01, 0, h01, h11, 0, 0, 0, 0];
        };
    
    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelHessianDiagonal = 
        (x, p) =>
        {
            var h11 = p[0] * x * x * Math.Exp(-p[1] * x);
            return [0, h11, 0];
        };

    // The following values are generated using the above model with parameters amplitude = 3, rate = 2, offset = 1,
    // adding random normal noise with a standard deviation of 0.01
    private static readonly IReadOnlyList<double> XValues =
    [
        0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1, 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7, 1.8, 1.9, 2, 2.1, 2.2,
        2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9
    ];

    private static readonly IReadOnlyList<double> YValues =
    [
        3.99, 3.45, 3.02, 2.65, 2.36, 2.11, 1.9, 1.75, 1.6, 1.49, 1.41, 1.32, 1.28, 1.22, 1.19, 1.15, 1.14, 1.09, 1.08,
        1.07, 1.03, 1.05, 1.05, 1.04, 1.03, 1.02, 1.03, 1.02, 1.02, 1.01
    ];
    
    private const double YError = 0.01;

    // Since the standard deviation of the noise overlying the data is chosen small enough, the optimal parameter
    // values are approximately equal to the values used to generate the data
    private static readonly IReadOnlyList<double> OptimumValues = [3, 2, 1];

    // Default parameter configurations
    private static readonly ParameterConfiguration DefaultAmplitude = Variable("amplitude", 3);
    private static readonly ParameterConfiguration DefaultRate = Variable("rate", 2, lowerLimit: 0);
    private static readonly ParameterConfiguration DefaultOffset = Variable("offset", 1);
}