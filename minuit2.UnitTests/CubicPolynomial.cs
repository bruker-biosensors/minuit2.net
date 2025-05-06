using minuit2.net;

namespace minuit2.UnitTests;

internal static class CubicPolynomial
{
    private static readonly Func<double, IList<double>, double> Model = 
        (x, c) => c[0] + c[1] * x + c[2] * x * x + c[3] * x * x * x;

    private static readonly Func<double, IList<double>, IList<double>> ModelGradient = 
        (x, _) => [1, x, x * x, x * x * x];
    
    
    // The following test data are generated using the above model with coefficients c0 = 10, c1 = -2, c2 = 1, c3 = -0.1,
    // adding random normal noise with a standard deviation of 0.1. These data are used to facilitate evaluation of
    // minimization results against independent fit libraries.
    private static readonly List<double> XValues = 
    [
        0, 0.5, 1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 5.5, 6, 6.5, 7, 7.5, 8, 8.5, 9, 9.5
    ];

    private static readonly List<double> YValues =
    [
        9.9, 9.2, 9.03, 8.93, 9.29, 9.75, 10.24, 11.02, 11.57, 12.11, 12.51, 12.46, 12.52, 11.72, 10.8, 9.08, 6.95,
        3.77, 0.07, -4.45
    ];

    private const double YError = 0.1;  // standard deviation of noise used to generate the above y-values
    
    public static LeastSquaresBuilder LeastSquaresCost => new();

    public class LeastSquaresBuilder
    {
        private string[] _parameterNames = ["c0", "c1", "c2", "c3"];
        private bool _hasYError = true;
        private bool _hasGradient;

        public LeastSquares Build() => _hasYError switch
        {
            true when _hasGradient => new LeastSquares(XValues, YValues, YError, _parameterNames, Model, ModelGradient),
            true when !_hasGradient => new LeastSquares(XValues, YValues, YError, _parameterNames, Model),
            false when _hasGradient => new LeastSquares(XValues, YValues, _parameterNames, Model, ModelGradient),
            _ => new LeastSquares(XValues, YValues, _parameterNames, Model)
        };

        public LeastSquaresBuilder WithGradient(bool hasGradient = true)
        {
            _hasGradient = hasGradient;
            return this;
        }

        public LeastSquaresBuilder WithParameterNames(string c0 = "c0", string c1 = "c1", string c2 = "c2", string c3 = "c3")
        {
            _parameterNames[0] = c0;
            _parameterNames[1] = c1;
            _parameterNames[2] = c2;
            _parameterNames[3] = c3;
            return this;
        }
        
        public LeastSquaresBuilder WithParameterSuffix(int suffix)
        {
            _parameterNames = _parameterNames.Select(p => $"{p}_{suffix}").ToArray();
            return this;
        }

        public LeastSquaresBuilder WithMissingYErrors()
        {
            _hasYError = false;
            return this;
        }
    }
    
    public static class ParameterConfigurations
    {
        public static ParameterConfiguration C0 => new("c0", 10.75);
        public static ParameterConfiguration C1 => new("c1", -1.97);
        public static ParameterConfiguration C2 => new("c2", 1.13);
        public static ParameterConfiguration C3 => new("c3", -0.11);
        public static ParameterConfiguration[] Defaults => [C0, C1, C2, C3];
        public static ParameterConfiguration[] DefaultsWithSuffix(int suffix) => Defaults.Select(p => p with { Name = $"{p.Name}_{suffix}" }).ToArray();
    }
}



