using minuit2.net;
using minuit2.net.CostFunctions;

namespace minuit2.UnitTests.TestUtilities;

internal static class QuadraticPolynomial
{
    private static readonly Func<double, IList<double>, double> Model = (x, c) => c[0] + c[1] * x + c[2] * x * x;

    private static readonly Func<double, IList<double>, IList<double>> ModelGradient = (x, _) => [1, x, x * x];
    
    
    // The following test data are generated using the above model with coefficients c0 = 10, c1 = -5, c2 = 0.5,
    // adding random normal noise with a standard deviation of 0.1. These data are used to facilitate evaluation of
    // minimization results against independent fit libraries.
    private static readonly List<double> XValues = 
    [
        0, 0.5, 1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 5.5, 6, 6.5, 7, 7.5, 8, 8.5, 9, 9.5
    ];

    private static readonly List<double> YValues =
    [
        9.9, 7.59, 5.63, 3.64, 2.09, 0.68, -0.56, -1.32, -2.03, -2.41, -2.49, -2.53, -1.88, -1.44, -0.4, 0.64, 2.15,
        3.56, 5.47, 7.66
    ];

    private const double YError = 0.1;  // standard deviation of noise used to generate the above y-values
    
    // Since the model is linear in its coefficients, the optimal parameter values are fully determined by the
    // closed-form solution of a linear regression (here, rounded to 2-digit precision).
    public static IReadOnlyCollection<double> OptimumParameterValues { get; } = [10, -5, 0.5];
    
    public static LeastSquaresBuilder LeastSquaresCost => new();

    public class LeastSquaresBuilder
    {
        private string[] _parameterNames = ["c0", "c1", "c2"];
        private bool _hasGradient;
        private double _errorDefinitionInSigma = 1;

        public ICostFunction Build() => _hasGradient 
            ? CostFunction.LeastSquares(XValues, YValues, YError, _parameterNames, Model, ModelGradient, _errorDefinitionInSigma) 
            : CostFunction.LeastSquares(XValues, YValues, YError, _parameterNames, Model, errorDefinitionInSigma: _errorDefinitionInSigma);

        public LeastSquaresBuilder WithGradient(bool hasGradient = true)
        {
            _hasGradient = hasGradient;
            return this;
        }
        
        public LeastSquaresBuilder WithParameterSuffix(int suffix)
        {
            _parameterNames = _parameterNames.Select(p => $"{p}_{suffix}").ToArray();
            return this;
        }
        
        public LeastSquaresBuilder WithErrorDefinition(double sigma)
        {
            _errorDefinitionInSigma = sigma;
            return this;
        }
    }
    
    public static class ParameterConfigurations
    {
        private static ParameterConfiguration C0 => ParameterConfiguration.Variable("c0", 10.90);
        private static ParameterConfiguration C1 => ParameterConfiguration.Variable("c1", -6.06);
        private static ParameterConfiguration C2 => ParameterConfiguration.Variable("c2", 0.58);
        public static ParameterConfiguration[] Defaults => [C0, C1, C2];
        public static ParameterConfiguration[] DefaultsWithSuffix(int suffix) => Defaults.Select(p => p.WithSuffix($"{suffix}")).ToArray();
    }
}