namespace minuit2.net;

public class LeastSquaresWithUniformYError : ICostFunctionRequiringErrorDefinitionAdjustment
{
    private readonly IList<double> _x;
    private readonly IList<double> _y;
    private readonly double _yError;
    private readonly Func<double, IList<double>, double> _model;
    private readonly Func<double, IList<double>, IList<double>>? _modelGradient;

    public LeastSquaresWithUniformYError(
        IList<double> x,
        IList<double> y,
        double yError,
        IList<string> parameters,
        Func<double, IList<double>, double> model,
        Func<double, IList<double>, IList<double>>? modelGradient = null, 
        double errorDefinitionScaling = 1)
    {
        if (x.Count != y.Count)
            throw new ArgumentException($"{nameof(x)} and {nameof(y)} must have the same length");
        
        _x = x;
        _y = y;
        _yError = yError;
        _model = model;
        _modelGradient = modelGradient;
        
        Parameters = parameters;
        HasGradient = modelGradient != null;
        ErrorDefinition = 1 * errorDefinitionScaling;  // TODO: Reuse constant
        RequiresErrorDefinitionAutoScaling = false;
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

    private double Residual(int i, IList<double> parameterValues) => (_y[i] - _model(_x[i], parameterValues)) / _yError;

    public IList<double> GradientFor(IList<double> parameterValues)
    {
        var gradientSums = new double[Parameters.Count];
        for (var i = 0; i < _x.Count; i++)
        {
            var factor = 2 * Residual(i, parameterValues);
            var gradients = _modelGradient!(_x[i], parameterValues);
            for (var j = 0; j < Parameters.Count; j++) 
                gradientSums[j] -= factor * gradients[j] / _yError;
        }
        
        return gradientSums;
    }
    
    public ICostFunctionRequiringErrorDefinitionAdjustment WithAutoScaledErrorDefinitionBasedOn(IList<double> parameterValues, IList<string> variables)
    {
        throw new NotImplementedException();
    }
}