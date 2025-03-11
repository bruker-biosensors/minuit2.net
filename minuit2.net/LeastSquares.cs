using System.Numerics;

namespace minuit2.net;

public class LeastSquares : ICostFunction, IAdditionOperators<LeastSquares, ICostFunction, ICostFunction>
{
    private readonly List<DataPoint> _data;
    private readonly Func<double, IList<double>, double> _model;
    private readonly bool _scaleCovariances;

    public LeastSquares(
        IList<double> x,
        IList<double> y,
        Func<double, IList<double>, double> model,
        IList<string> parameters)
        : this(x, y, 1.0, model, parameters, scaleCovariances: true) { }
    
    public LeastSquares(
        IList<double> x,
        IList<double> y,
        double yError,
        Func<double, IList<double>, double> model,
        IList<string> parameters, 
        bool scaleCovariances = false)
        : this(x, y, Enumerable.Repeat(yError, y.Count).ToList(), model, parameters, scaleCovariances) { }

    private LeastSquares(
        IList<double> x, 
        IList<double> y, 
        IList<double> yError,
        Func<double, IList<double>, double> model, 
        IList<string> parameters, 
        bool scaleCovariances)
    {
        if (x.Count != y.Count || x.Count != yError.Count)
            throw new ArgumentException($"{nameof(x)}, {nameof(y)} and {nameof(yError)} must have the same length");

        if (CannotBeMapped(parameters, model))
            throw new ArgumentException($"{nameof(parameters)} has fewer elements than the number of parameters in {nameof(model)}");
        
        _data = x.Zip(y, yError).Select(t => new DataPoint(t.First, t.Second, t.Third)).ToList();
        _model = model;
        _scaleCovariances = scaleCovariances;
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
    
    public double CovarianceScaleFactorFor(double costValue, int numberOfVariables)
    {
        if (!_scaleCovariances) return 1;
        
        // Auto-scale the covariances to match the values obtained when data uncertainties are estimated such that
        // the reduced chi-squared equals 1. This is the default behaviour in lmfit.
        // source: https://lmfit.github.io/lmfit-py/fitting.html#uncertainties-in-variable-parameters-and-their-correlations
        var degreesOfFreedom = _data.Count - numberOfVariables;
        var reducedChiSquared = costValue / degreesOfFreedom;
        return reducedChiSquared;
    }

    public static ICostFunction operator +(LeastSquares left, ICostFunction right) => new CostFunctionSum(left, right);
}
