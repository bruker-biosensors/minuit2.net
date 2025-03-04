namespace minuit2.net;

public class LeastSquares : ICostFunction
{
    private readonly List<DataPoint> _data;
    private readonly Func<double, IList<double>, double> _model;

    public LeastSquares(IList<double> x, IList<double> y, double yError, Func<double, IList<double>, double> model)
        : this(x, y, Enumerable.Repeat(yError, y.Count).ToList(), model) { }

    private LeastSquares(IList<double> x, IList<double> y, IList<double> yError, Func<double, IList<double>, double> model)
    {
        if (x.Count != y.Count || x.Count != yError.Count)
            throw new ArgumentException($"{nameof(x)}, {nameof(y)} and {nameof(yError)} must have the same length");
        
        _data = x.Zip(y, yError).Select(t => new DataPoint(t.First, t.Second, t.Third)).ToList();
        _model = model;
    }
    
    public double ValueFor(IList<double> parameters) => _data
        .Select(datum => (datum.Y - _model(datum.X, parameters)) / datum.YError)
        .Select(residual => residual * residual)
        .Sum();
}