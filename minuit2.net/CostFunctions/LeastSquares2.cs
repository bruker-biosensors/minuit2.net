namespace minuit2.net.CostFunctions;

internal class LeastSquares2 : ICostFunction
{
    private readonly IReadOnlyList<double> _x;
    private readonly IReadOnlyList<double> _y;
    private readonly Func<int, double> _yErrorForIndex;
    private readonly Func<IReadOnlyList<double>, IReadOnlyList<double>, IReadOnlyList<double>> _model;
    private readonly double _errorDefinitionInSigma;
    private readonly bool _isErrorDefinitionRecalculationEnabled;

    public LeastSquares2(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        Func<int, double> yErrorForIndex,
        IReadOnlyList<string> parameters,
        Func<IReadOnlyList<double>, IReadOnlyList<double>, IReadOnlyList<double>> model,
        double errorDefinitionInSigma,
        bool isErrorDefinitionRecalculationEnabled)
        : this(x, y, yErrorForIndex, parameters, model, errorDefinitionInSigma, isErrorDefinitionRecalculationEnabled, 1)
    {
    }

    private LeastSquares2(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        Func<int, double> yErrorForIndex,
        IReadOnlyList<string> parameters,
        Func<IReadOnlyList<double>, IReadOnlyList<double>, IReadOnlyList<double>> model,
        double errorDefinitionInSigma,
        bool isErrorDefinitionRecalculationEnabled,
        double errorDefinitionScaling)
    {
        if (x.Count != y.Count)
            throw new ArgumentException($"{nameof(x)} and {nameof(y)} must have the same length");

        _x = x;
        _y = y;
        _yErrorForIndex = yErrorForIndex;
        _model = model;
        _errorDefinitionInSigma = errorDefinitionInSigma;
        _isErrorDefinitionRecalculationEnabled = isErrorDefinitionRecalculationEnabled;

        Parameters = parameters;
        HasGradient = false;

        // For least squares fits, an error definition of 1 corresponds to 1-sigma parameter errors
        // (4 would correspond to 2-sigma errors, 9 would correspond to 3-sigma errors etc.)
        ErrorDefinition = errorDefinitionInSigma * errorDefinitionInSigma * errorDefinitionScaling;
    }

    public IReadOnlyList<string> Parameters { get; }
    public bool HasGradient { get; }
    public double ErrorDefinition { get; }

    public double ValueFor(IReadOnlyList<double> parameterValues)
    {
        double sum = 0;
        var yModel = _model(_x, parameterValues);
        for (var i = 0; i < _x.Count; i++)
        {
            var residual = (_y[i] - yModel[i]) / _yErrorForIndex(i);
            sum += residual * residual;
        }

        return sum;
    }

    public IReadOnlyList<double> GradientFor(IReadOnlyList<double> parameterValues)
    {
        throw new NotImplementedException();
    }

    public ICostFunction WithErrorDefinitionRecalculatedBasedOnValid(IMinimizationResult result)
    {
        if (!_isErrorDefinitionRecalculationEnabled) return this;

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
        return new LeastSquares2(
            _x,
            _y,
            _yErrorForIndex,
            Parameters,
            _model,
            _errorDefinitionInSigma,
            _isErrorDefinitionRecalculationEnabled,
            reducedChiSquared);
    }
}