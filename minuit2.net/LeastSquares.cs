namespace minuit2.net;

public class LeastSquares : ICostFunction
{
    // For chi-squared fits Up = 1 corresponds to standard 1-sigma parameter errors
    // (Up = 4 would correspond to 2-sigma errors).
    private const double ChiSquaredUp = 1;
    
    private readonly List<DataPoint> _data;
    private readonly Func<double, IList<double>, double> _model;
    private readonly Func<double, IList<double>, IList<double>>? _modelGradient;

    public LeastSquares(
        IList<double> x,
        IList<double> y,
        IList<string> parameters,
        Func<double, IList<double>, double> model, 
        Func<double, IList<double>, IList<double>>? modelGradient = null)
        : this(x, y, 1.0, parameters, model, modelGradient, shouldScaleCovariances: true) { }
    
    public LeastSquares(
        IList<double> x,
        IList<double> y,
        double yError,
        IList<string> parameters,
        Func<double, IList<double>, double> model,
        Func<double, IList<double>, IList<double>>? modelGradient = null,
        bool shouldScaleCovariances = false)
        : this(x, y, Enumerable.Repeat(yError, y.Count).ToList(), parameters, model, modelGradient, shouldScaleCovariances) { }
    
    private LeastSquares(
        IList<double> x,
        IList<double> y,
        IList<double> yError,
        IList<string> parameters,
        Func<double, IList<double>, double> model,
        Func<double, IList<double>, IList<double>>? modelGradient,
        bool shouldScaleCovariances)
    {
        if (x.Count != y.Count || x.Count != yError.Count)
            throw new ArgumentException($"{nameof(x)}, {nameof(y)} and {nameof(yError)} must have the same length");
        
        _data = x.Zip(y, yError).Select(t => new DataPoint(t.First, t.Second, t.Third)).ToList();
        _model = model;
        _modelGradient = modelGradient;
        
        Parameters = parameters;
        HasGradient = modelGradient != null;
        ErrorDefinition = ChiSquaredUp;
        
        NumberOfData = x.Count;
        ShouldScaleCovariances = shouldScaleCovariances;
    }

    public IList<string> Parameters { get; }
    public bool HasGradient { get; }
    public double ErrorDefinition { get; }
    
    internal int NumberOfData { get; }
    internal bool ShouldScaleCovariances { get; }

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
    
    public MinimizationResult Adjusted(MinimizationResult minimizationResult)
    {
        if (!ShouldScaleCovariances) return minimizationResult;
        
        // Auto-scale the covariances to match the values that would be obtained when data uncertainties were
        // chosen such that the reduced chi-squared becomes 1. This is the default behaviour in lmfit.
        // source: https://lmfit.github.io/lmfit-py/fitting.html#uncertainties-in-variable-parameters-and-their-correlations
        var degreesOfFreedom = NumberOfData - minimizationResult.NumberOfVariables;
        var reducedChiSquared = minimizationResult.CostValue / degreesOfFreedom;
        return minimizationResult.WithParameterCovariancesScaledBy(reducedChiSquared);
    }
}