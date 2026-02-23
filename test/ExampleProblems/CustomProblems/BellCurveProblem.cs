using minuit2.net;
using static ExampleProblems.DerivativeConfiguration;
using static minuit2.net.ParameterConfiguration;

namespace ExampleProblems.CustomProblems;

public class BellCurveProblem(
    ParameterConfiguration? location = null,
    ParameterConfiguration? variance = null,
    DerivativeConfiguration derivativeConfiguration = WithoutDerivatives)
    : LeastSquaresProblem(
        XValues,
        YValues,
        YError,
        [location ?? DefaultLocation, variance ?? DefaultVariance],
        OptimumValues,
        Model,
        ModelGradient,
        ModelHessian,
        ModelHessianDiagonal,
        derivativeConfiguration)
{
    private static double Bell(double x, double location, double variance)
    {
        var dx = x - location;
        return 1 / Math.Sqrt(2 * Math.PI * variance) * Math.Exp(-dx * dx / (2 * variance));
    }
    
    private static readonly Func<double, IReadOnlyList<double>, double> Model = 
        (x, p) => Bell(x, p[0], p[1]);

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelGradient = 
        (x, p) =>
        {
            var frac = (x - p[0]) / p[1];
            var g1 = frac * Bell(x, p[0], p[1]);
            var g2 = (frac * frac - 1 / p[1]) * Bell(x, p[0], p[1]);
            return [g1, g2];
        };

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelHessian = 
        (x, p) =>
        {
            var frac = (x - p[0]) / p[1];
            var h00 = (frac * frac - 1 / p[1]) * Bell(x, p[0], p[1]);
            var h01 = frac / 2 * (frac * frac - 3 / p[1]) * Bell(x, p[0], p[1]);
            var h11 = ((frac * frac - 1 / p[1]) * (frac * frac - 1 / p[1]) / 4 - (frac * frac - 1 / (2 * p[1])) / p[1]) * Bell(x, p[0], p[1]);
            return [h00, h01, h01, h11];
        };
    
    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelHessianDiagonal = 
        (x, p) =>
        {
            var frac = (x - p[0]) / p[1];
            var h00 = (frac * frac - 1 / p[1]) * Bell(x, p[0], p[1]);
            var h11 = ((frac * frac - 1 / p[1]) * (frac * frac - 1 / p[1]) / 4 - (frac * frac - 1 / (2 * p[1])) / p[1]) * Bell(x, p[0], p[1]);
            return [h00, h11];
        };
    
    // The following values are generated using the above model with parameters location = 5, variance = 2,
    // adding random normal noise with a standard deviation of 0.001
    private static readonly IReadOnlyList<double> XValues =
    [
        0, 0.2, 0.4, 0.6, 0.8, 1, 1.2, 1.4, 1.6, 1.8, 2, 2.2, 2.4, 2.6, 2.8, 3, 3.2, 3.4, 3.6, 3.8, 4, 4.2, 4.4, 4.6,
        4.8, 5, 5.2, 5.4, 5.6, 5.8, 6, 6.2, 6.4, 6.6, 6.8, 7, 7.2, 7.4, 7.6, 7.8, 8, 8.2, 8.4, 8.6, 8.8, 9, 9.2, 9.4,
        9.6, 9.8
    ];

    private static readonly IReadOnlyList<double> YValues =
    [
        0, 0.001, 0.003, 0.002, 0.004, 0.006, 0.007, 0.012, 0.015, 0.021, 0.03, 0.038, 0.053, 0.066, 0.085, 0.104,
        0.127, 0.148, 0.173, 0.197, 0.217, 0.241, 0.259, 0.272, 0.28, 0.282, 0.281, 0.272, 0.258, 0.24, 0.219, 0.196,
        0.174, 0.147, 0.125, 0.104, 0.085, 0.069, 0.053, 0.039, 0.03, 0.021, 0.017, 0.011, 0.007, 0.005, 0.003, 0.002,
        0.004, 0
    ];

    private const double YError = 0.001;

    // Since the standard deviation of the noise overlying the data is chosen small enough, the optimal parameter
    // values are approximately equal to the values used to generate the data
    private static readonly IReadOnlyList<double> OptimumValues = [5, 2];
    
    // Default parameter configurations
    private static readonly ParameterConfiguration DefaultLocation = Variable("location", 4);
    private static readonly ParameterConfiguration DefaultVariance = Variable("variance", 3, lowerLimit: 0);
}