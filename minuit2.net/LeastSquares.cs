namespace minuit2.net;

public class LeastSquares : ILeastSquares
{
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

        if (CannotBeMapped(parameters, model))
            throw new ArgumentException($"{nameof(parameters)} has fewer elements than the number of parameters in {nameof(model)}");
        
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
    
    private static bool CannotBeMapped(IList<string> parameters, Func<double, IList<double>, double> model)
    {
        try
        {
            _ = model(double.NaN, Enumerable.Repeat(double.NaN, parameters.Count).ToList());
            return false;
        }
        catch (Exception)
        {
            return true;
        }
    }

    public double ValueFor(IList<double> parameterValues) => _data
        .Select(datum => (datum.Y - _model(datum.X, parameterValues)) / datum.YError)
        .Select(residual => residual * residual)
        .Sum();
    
    public static LeastSquaresSum operator +(LeastSquares left, ILeastSquares right) => new(left, right);
}
