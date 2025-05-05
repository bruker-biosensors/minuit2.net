using minuit2.net;

namespace minuit2.UnitTests;

internal static class CubicPolynomial
{
    public static readonly Func<double, IList<double>, double> Model = 
        (x, c) => c[0] + c[1] * x + c[2] * x * x + c[3] * x * x * x;

    public static readonly Func<double, IList<double>, IList<double>> ModelGradient = 
        (x, _) => [1, x, x * x, x * x * x];
    
    
    // The following test data are generated using the above model with coefficients c0 = 10, c1 = -2, c2 = 1, c3 = -0.1,
    // adding random normal noise with a standard deviation of 0.1. These data are used to evaluate minimization results
    // against independent fit libraries.
    public static readonly List<double> XValues = 
    [
        0, 0.5, 1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 5.5, 6, 6.5, 7, 7.5, 8, 8.5, 9, 9.5
    ];

    public static readonly List<double> YValues =
    [
        9.9, 9.2, 9.03, 8.93, 9.29, 9.75, 10.24, 11.02, 11.57, 12.11, 12.51, 12.46, 12.52, 11.72, 10.8, 9.08, 6.95,
        3.77, 0.07, -4.45
    ];

    public const double YError = 0.1;  // standard deviation of noise used to generate the above y-values
    
    
    // Default parameter configurations used to initialize the test minimization;
    // The parameter name disorder is intentional to ensure correct parameter configuration-to-model parameter mapping
    public static readonly ParameterConfiguration[] DefaultParameterConfigurations = 
    [
        new("c3", -0.11),
        new("c2", 1.13),
        new("c0", 10.75),
        new("c1", -1.97)
    ];
}