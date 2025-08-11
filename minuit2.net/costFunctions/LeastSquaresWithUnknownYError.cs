namespace minuit2.net.costFunctions;

public class LeastSquaresWithUnknownYError : ICostFunctionRequiringErrorDefinitionAdjustment
{
    private readonly IList<double> _x;
    private readonly IList<double> _y;
    private readonly Func<double, IList<double>, double> _model;
    private readonly Func<double, IList<double>, IList<double>>? _modelGradient;

    public LeastSquaresWithUnknownYError(
        IList<double> x,
        IList<double> y,
        IList<string> parameters,
        Func<double, IList<double>, double> model,
        Func<double, IList<double>, IList<double>>? modelGradient = null, 
        double errorDefinitionScaling = 1)
    {
        if (x.Count != y.Count)
            throw new ArgumentException($"{nameof(x)} and {nameof(y)} must have the same length");
        
        _x = x;
        _y = y;
        _model = model;
        _modelGradient = modelGradient;
        
        Parameters = parameters;
        HasGradient = modelGradient != null;
        ErrorDefinition = 1 * errorDefinitionScaling;  // TODO: Reuse constant
        RequiresErrorDefinitionAutoScaling = true;
    }
    
    public IList<string> Parameters { get; }
    public bool HasGradient { get; }
    public double ErrorDefinition { get; }
    public bool RequiresErrorDefinitionAutoScaling { get; }
    
    public double ValueFor(IList<double> parameterValues)
    {
        double sum = 0;
        for (var i = 0; i < _x.Count; i++)
        {
            var residual = Residual(i, parameterValues);
            sum += residual * residual;
        }
        
        return sum;
    }

    private double Residual(int i, IList<double> parameterValues) => (_y[i] - _model(_x[i], parameterValues));

    public IList<double> GradientFor(IList<double> parameterValues)
    {
        var gradientSums = new double[Parameters.Count];
        for (var i = 0; i < _x.Count; i++)
        {
            var factor = 2 * Residual(i, parameterValues);
            var gradients = _modelGradient!(_x[i], parameterValues);
            for (var j = 0; j < Parameters.Count; j++) 
                gradientSums[j] -= factor * gradients[j];
        }
        
        return gradientSums;
    }
    
    public ICostFunctionRequiringErrorDefinitionAdjustment WithAutoScaledErrorDefinitionBasedOn(IList<double> parameterValues, IList<string> variables)
    {
        // Auto-scale the error definition such that a re-evaluation -- e.g. by a subsequent minimization or accurate
        // covariance computation (Hesse algorithm) -- yields the same parameter covariances that would be obtained
        // from a minimization using data y-errors resulting in a reduced chi-squared value of 1.
        // Yet, in contrast to scaling the y-errors, by scaling the error definition the cost value (chi-squared value)
        // itself won't be affected. This means that for missing y-errors, the reduced chi-squared should approximate
        // the variance of the noise overlying the data.
        // This is equivalent to the default behaviour in lmfit:
        // https://lmfit.github.io/lmfit-py/fitting.html#uncertainties-in-variable-parameters-and-their-correlations
        
        var degreesOfFreedom = _x.Count - variables.Count;
        var reducedChiSquared = ValueFor(parameterValues) / degreesOfFreedom;
        return new LeastSquaresWithUnknownYError(_x, _y, Parameters, _model, _modelGradient, reducedChiSquared);
    }
}