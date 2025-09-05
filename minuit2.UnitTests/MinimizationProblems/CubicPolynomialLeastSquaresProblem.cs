using System.Diagnostics.CodeAnalysis;
using minuit2.net;
using minuit2.net.CostFunctions;
using minuit2.UnitTests.TestUtilities;

namespace minuit2.UnitTests.MinimizationProblems;

internal class CubicPolynomialLeastSquaresProblem : CubicPolynomialLeastSquaresProblemBase
{
    public override ICostFunction Cost { get; } = DefaultCost.Build();

    public override IReadOnlyCollection<ParameterConfiguration> ParameterConfigurations =>
        Cost.Parameters.Zip(OptimumParameterValues,
            (name, optimumValue) => ParameterConfiguration.Variable(name, InitialValueFor(optimumValue))).ToArray();

    private static double InitialValueFor(double optimumValue, double maximumRelativeBias = 0.1)
    {
        var maximumBias = Math.Abs(optimumValue) * maximumRelativeBias;
        return Any.Double().Between(optimumValue - maximumBias, optimumValue + maximumBias);
    }
}

internal abstract class CubicPolynomialLeastSquaresProblemBase : IMinimizationProblem
{
    private static readonly Func<double, IList<double>, double> Model = 
        (x, c) => c[0] + c[1] * x + c[2] * x * x + c[3] * x * x * x;

    private static readonly Func<double, IList<double>, IList<double>> ModelGradient = 
        (x, _) => [1, x, x * x, x * x * x];
    
    // The following values are generated using the above model with coefficients c0 = 10, c1 = -2, c2 = 1, c3 = -0.1,
    // adding random normal noise with a standard deviation of 0.1.
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

    // Since the model is linear in its coefficients, the optimal parameter values are fully determined by the
    // closed-form solution of a linear regression.
    public virtual IReadOnlyCollection<double> OptimumParameterValues { get; } = [9.97, -1.96, 0.99, -0.1];
    
    public abstract IReadOnlyCollection<ParameterConfiguration> ParameterConfigurations { get; }
    
    public abstract ICostFunction Cost { get; }
    
    protected static CostBuilder DefaultCost => new();

    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Will be used in a next step")]
    protected class CostBuilder
    {
        private string[] _parameterNames = ["c0", "c1", "c2", "c3"];
        private bool _hasYErrors = true;
        private bool _hasGradient;
        private double _errorDefinitionInSigma = 1;
        
        public ICostFunction Build() => _hasYErrors switch
        {
            true when _hasGradient => CostFunction.LeastSquares(XValues, YValues, YError, _parameterNames, Model, ModelGradient, _errorDefinitionInSigma),
            true when !_hasGradient => CostFunction.LeastSquares(XValues, YValues, YError, _parameterNames, Model, errorDefinitionInSigma: _errorDefinitionInSigma),
            false when _hasGradient => CostFunction.LeastSquares(XValues, YValues, _parameterNames, Model, ModelGradient, _errorDefinitionInSigma),
            _ => CostFunction.LeastSquares(XValues, YValues, _parameterNames, Model, errorDefinitionInSigma: _errorDefinitionInSigma)
        };
        
        public CostBuilder WithUnknownYErrors()
        {
            _hasYErrors = false;
            return this;
        }
        
        public CostBuilder WithGradient(bool hasGradient = true)
        {
            _hasGradient = hasGradient;
            return this;
        }

        public CostBuilder WithParameterNames(string c0 = "c0", string c1 = "c1", string c2 = "c2", string c3 = "c3")
        {
            _parameterNames[0] = c0;
            _parameterNames[1] = c1;
            _parameterNames[2] = c2;
            _parameterNames[3] = c3;
            return this;
        }
        
        public CostBuilder WithParameterSuffix(int suffix)
        {
            _parameterNames = _parameterNames.Select(p => $"{p}_{suffix}").ToArray();
            return this;
        }
        
        public CostBuilder WithErrorDefinition(double sigma)
        {
            _errorDefinitionInSigma = sigma;
            return this;
        }
        
        public CostBuilder WithAnyErrorDefinitionBetween(double lowSigma, double highSigma)
        {
            _errorDefinitionInSigma = Any.Double().Between(lowSigma, highSigma);
            return this;
        }
    }
}