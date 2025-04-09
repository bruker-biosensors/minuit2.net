namespace minuit2.net;

public class LeastSquares : ILeastSquares
{
    // For chi-squared fits Up = 1 corresponds to standard 1-sigma parameter errors
    // (Up = 4 would correspond to 2-sigma errors).
    internal const double ChiSquaredUp = 1;
    
    private readonly List<DataPoint> _data;
    private readonly Func<double, IList<double>, double> _model;
    
    public LeastSquares(
        IList<double> x,
        IList<double> y,
        Func<double, IList<double>, double> model,
        IList<string> parameters)
        : this(x, y, 1.0, model, parameters, shouldScaleCovariances: true) { }
    
    public LeastSquares(
        IList<double> x,
        IList<double> y,
        double yError,
        Func<double, IList<double>, double> model,
        IList<string> parameters, 
        bool shouldScaleCovariances = false)
        : this(x, y, Enumerable.Repeat(yError, y.Count).ToList(), model, parameters, shouldScaleCovariances) { }

    private LeastSquares(
        IList<double> x, 
        IList<double> y, 
        IList<double> yError,
        Func<double, IList<double>, double> model, 
        IList<string> parameters, 
        bool shouldScaleCovariances)
    {
        if (x.Count != y.Count || x.Count != yError.Count)
            throw new ArgumentException($"{nameof(x)}, {nameof(y)} and {nameof(yError)} must have the same length");
        
        _data = x.Zip(y, yError).Select(t => new DataPoint(t.First, t.Second, t.Third)).ToList();
        _model = model;
        
        Parameters = parameters;
        NumberOfData = x.Count;
        ShouldScaleCovariances = shouldScaleCovariances;
    }
    
    internal static LeastSquares Seed => new([], [], [], (_, _) => 0, [], false);

    public IList<string> Parameters { get; }
    public int NumberOfData { get; }
    public bool ShouldScaleCovariances { get; }

    public double ValueFor(IList<double> parameterValues) => _data
        .Select(datum => (datum.Y - _model(datum.X, parameterValues)) / datum.YError)
        .Select(residual => residual * residual)
        .Sum();

    public IList<double> GradientFor(IList<double> parameters)
    {
        throw new NotImplementedException();
    }

    public bool HasGradient => false;

    public double Up => ChiSquaredUp;

    public static LeastSquaresSum operator +(LeastSquares left, ILeastSquares right) => new(left, right);
}
