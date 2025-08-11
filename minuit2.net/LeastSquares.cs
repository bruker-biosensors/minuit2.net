namespace minuit2.net;

public class LeastSquares : ICostFunctionRequiringErrorDefinitionAdjustment
{
    // For chi-squared fits, ErrorDefinition = 1 corresponds to standard 1-sigma parameter errors
    // (ErrorDefinition = 4 would correspond to 2-sigma errors etc.)
    private const double ChiSquaredErrorDefinition = 1;
    
    private readonly IList<double> _x;
    private readonly IList<double> _y;
    private readonly IList<double> _yError;
    private readonly List<DataPoint> _data;
    private readonly Func<double, IList<double>, double> _model;
    private readonly Func<double, IList<double>, IList<double>>? _modelGradient;
    
    public LeastSquares(
        IList<double> x,
        IList<double> y,
        IList<double> yError,
        IList<string> parameters,
        Func<double, IList<double>, double> model,
        Func<double, IList<double>, IList<double>>? modelGradient = null,
        double errorDefinitionScaling = 1)
    {
        if (x.Count != y.Count || x.Count != yError.Count)
            throw new ArgumentException($"{nameof(x)}, {nameof(y)} and {nameof(yError)} must have the same length");
        
        _x = x;
        _y = y;
        _yError = yError;
        _data = x.Zip(y, yError).Select(t => new DataPoint(t.First, t.Second, t.Third)).ToList();
        _model = model;
        _modelGradient = modelGradient;
        
        Parameters = parameters;
        HasGradient = modelGradient != null;
        ErrorDefinition = ChiSquaredErrorDefinition * errorDefinitionScaling;
        RequiresErrorDefinitionAutoScaling = false;
    }

    public IList<string> Parameters { get; }
    public bool HasGradient { get; }
    public double ErrorDefinition { get; }
    public bool RequiresErrorDefinitionAutoScaling { get; }

    public double ValueFor(IList<double> parameterValues) => 
        _data.Select(datum => ResidualFor(datum, parameterValues)).Select(residual => residual * residual).Sum();

    public IList<double> GradientFor(IList<double> parameterValues)
    {
        var gradientSums = Enumerable.Repeat(0d, Parameters.Count).ToList();
        foreach (var datum in _data)
        {
            var gradients = _modelGradient!(datum.X, parameterValues);
            var factor = 2 * ResidualFor(datum, parameterValues);
            for (var i = 0; i < Parameters.Count; i++) 
                gradientSums[i] -= factor * gradients[i] / datum.YError;          
        }

        return gradientSums;
    }

    private double ResidualFor(DataPoint datum, IList<double> parameterValues) =>
        (datum.Y - _model(datum.X, parameterValues)) / datum.YError;

    public ICostFunctionRequiringErrorDefinitionAdjustment WithAutoScaledErrorDefinitionBasedOn(IList<double> parameterValues, IList<string> variables)
    {
        throw new NotImplementedException();
    }
}