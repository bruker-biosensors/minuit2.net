namespace minuit2.net.costFunctions;

public class LeastSquares : ICostFunction
{
    // For least squares fits, an error definition of 1 corresponds to 1-sigma parameter errors
    // (4 would correspond to 2-sigma errors, 9 would correspond to 3-sigma errors etc.)
    public const double OneSigmaErrorDefinition = 1;
    
    private readonly List<DataPoint> _data;
    private readonly Func<double, IList<double>, double> _model;
    private readonly Func<double, IList<double>, IList<double>>? _modelGradient;
    
    public LeastSquares(
        IList<double> x,
        IList<double> y,
        IList<double> yError,
        IList<string> parameters,
        Func<double, IList<double>, double> model,
        Func<double, IList<double>, IList<double>>? modelGradient = null)
    {
        if (x.Count != y.Count || x.Count != yError.Count)
            throw new ArgumentException($"{nameof(x)}, {nameof(y)} and {nameof(yError)} must have the same length");

        _data = x.Zip(y, yError).Select(t => new DataPoint(t.First, t.Second, t.Third)).ToList();
        _model = model;
        _modelGradient = modelGradient;
        
        Parameters = parameters;
        HasGradient = modelGradient != null;
        ErrorDefinition = OneSigmaErrorDefinition;
    }

    public IList<string> Parameters { get; }
    public bool HasGradient { get; }
    public double ErrorDefinition { get; }
    
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
}