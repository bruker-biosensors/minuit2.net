namespace minuit2.net.CostFunctions;

internal class LeastSquaresWithUnknownYError : ICostFunction
{
    private readonly IReadOnlyList<double> _x;
    private readonly IReadOnlyList<double> _y;
    private readonly Func<double, IReadOnlyList<double>, double> _model;
    private readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? _modelGradient;
    private readonly double _errorDefinitionInSigma;

    public LeastSquaresWithUnknownYError(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelGradient = null,
        double errorDefinitionInSigma = 1,
        double errorDefinitionScaling = 1)
    {
        if (x.Count != y.Count)
            throw new ArgumentException($"{nameof(x)} and {nameof(y)} must have the same length");
        
        _x = x;
        _y = y;
        _model = model;
        _modelGradient = modelGradient;
        _errorDefinitionInSigma = errorDefinitionInSigma;
        
        Parameters = parameters;
        HasGradient = modelGradient != null;
        ErrorDefinition = LeastSquares.ErrorDefinitionFor(errorDefinitionInSigma) * errorDefinitionScaling;
    }
    
    public IReadOnlyList<string> Parameters { get; }
    public bool HasGradient { get; }
    public double ErrorDefinition { get; }
    
    public double ValueFor(IReadOnlyList<double> parameterValues)
    {
        double sum = 0;
        for (var i = 0; i < _x.Count; i++)
        {
            var residual = ResidualFor(i, parameterValues);
            sum += residual * residual;
        }
        
        return sum;
    }
    
    public IReadOnlyList<double> GradientFor(IReadOnlyList<double> parameterValues)
    {
        var gradientSums = new double[Parameters.Count];
        for (var i = 0; i < _x.Count; i++)
        {
            var factor = 2 * ResidualFor(i, parameterValues);
            var gradients = _modelGradient!(_x[i], parameterValues);
            for (var j = 0; j < Parameters.Count; j++) 
                gradientSums[j] -= factor * gradients[j];
        }
        
        return gradientSums;
    }

    public ICostFunction WithErrorDefinitionRecalculatedBasedOnValid(IMinimizationResult result)
    {
        // Auto-scale the error definition such that a re-evaluation -- e.g., by a following minimization or accurate
        // covariance computation (Hesse algorithm) -- yields the same parameter covariances that would be obtained
        // from a minimization using an "ideal y-error" resulting in a reduced chi-squared value of 1.
        
        // By scaling the error definition, the cost value (chi-squared value) itself won't be affected. This means
        // that the resulting reduced chi-squared should approximate the variance of the noise overlying the data.
        
        // This is equivalent to the default behavior in lmfit:
        // https://lmfit.github.io/lmfit-py/fitting.html#uncertainties-in-variable-parameters-and-their-correlations

        var resultParameters = result.Parameters.ToList();
        var parameterValues = Parameters.Select(p => result.ParameterValues[resultParameters.IndexOf(p)]).ToArray();
        var numberOfVariables = Parameters.Count(p => result.Variables.Contains(p));
        
        var degreesOfFreedom = _x.Count - numberOfVariables;
        var reducedChiSquared = ValueFor(parameterValues) / degreesOfFreedom;
        return new LeastSquaresWithUnknownYError(
            _x,
            _y,
            Parameters,
            _model,
            _modelGradient,
            _errorDefinitionInSigma,
            reducedChiSquared);
    }

    private double ResidualFor(int i, IReadOnlyList<double> parameterValues) => _y[i] - _model(_x[i], parameterValues);
}