using System.Numerics;

namespace minuit2.net;

public class LeastSquares : ICostFunction, IAdditionOperators<LeastSquares, ICostFunction, ICostFunction>
{
    private readonly List<DataPoint> _data;
    private readonly Func<double, IList<double>, double> _model;

    public LeastSquares(
        IList<double> x,
        IList<double> y,
        double yError,
        Func<double, IList<double>, double> model,
        IList<string> parameters)
        : this(x, y, Enumerable.Repeat(yError, y.Count).ToList(), model, parameters) { }

    private LeastSquares(
        IList<double> x, 
        IList<double> y, 
        IList<double> yError,
        Func<double, IList<double>, double> model, 
        IList<string> parameters)
    {
        if (x.Count != y.Count || x.Count != yError.Count)
            throw new ArgumentException($"{nameof(x)}, {nameof(y)} and {nameof(yError)} must have the same length");

        if (CannotBeMapped(parameters, model))
            throw new ArgumentException($"{nameof(parameters)} has fewer elements than the number of parameters in {nameof(model)}");
        
        _data = x.Zip(y, yError).Select(t => new DataPoint(t.First, t.Second, t.Third)).ToList();
        _model = model;
        Parameters = parameters;
    }

    public IList<string> Parameters { get; }

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

    public static ICostFunction operator +(LeastSquares left, ICostFunction right) => new CostFunctionSum(left, right);
}
