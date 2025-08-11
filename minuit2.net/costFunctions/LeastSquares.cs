namespace minuit2.net.costFunctions;

internal class LeastSquares : ICostFunction
{
    private readonly IList<double> _x;
    private readonly IList<double> _y;
    private readonly IList<double> _yError;
    private readonly Func<double, IList<double>, double> _model;
    private readonly Func<double, IList<double>, IList<double>>? _modelGradient;
    
    public LeastSquares(
        IList<double> x,
        IList<double> y,
        IList<double> yError,
        IList<string> parameters,
        Func<double, IList<double>, double> model,
        Func<double, IList<double>, IList<double>>? modelGradient = null, 
        double errorDefinitionInSigma = 1)
    {
        if (x.Count != y.Count || x.Count != yError.Count)
            throw new ArgumentException($"{nameof(x)}, {nameof(y)} and {nameof(yError)} must have the same length");

        _x = x;
        _y = y;
        _yError = yError;
        _model = model;
        _modelGradient = modelGradient;
        
        Parameters = parameters;
        HasGradient = modelGradient != null;
        ErrorDefinition = ErrorDefinitionFor(errorDefinitionInSigma);
    }

    public IList<string> Parameters { get; }
    public bool HasGradient { get; }
    public double ErrorDefinition { get; }
    
    public double ValueFor(IList<double> parameterValues)
    {
        double sum = 0;
        for (var i = 0; i < _x.Count; i++)
        {
            var residual = ResidualFor(i, parameterValues);
            sum += residual * residual;
        }
        
        return sum;
    }

    public IList<double> GradientFor(IList<double> parameterValues)
    {
        var gradientSums = new double[Parameters.Count];
        for (var i = 0; i < _x.Count; i++)
        {
            var factor = 2 * ResidualFor(i, parameterValues);
            var gradients = _modelGradient!(_x[i], parameterValues);
            for (var j = 0; j < Parameters.Count; j++) 
                gradientSums[j] -= factor * gradients[j] / _yError[i];
        }
        
        return gradientSums;
    }

    private double ResidualFor(int i, IList<double> parameterValues) =>
        (_y[i] - _model(_x[i], parameterValues)) / _yError[i];
    
    // For least squares fits, an error definition of 1 corresponds to 1-sigma parameter errors
    // (4 would correspond to 2-sigma errors, 9 would correspond to 3-sigma errors etc.)
    internal static double ErrorDefinitionFor(double errorDefinitionInSigma) =>
        errorDefinitionInSigma * errorDefinitionInSigma;
}