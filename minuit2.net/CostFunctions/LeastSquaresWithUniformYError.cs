namespace minuit2.net.CostFunctions;

internal class LeastSquaresWithUniformYError : LeastSquaresBase
{
    private readonly IReadOnlyList<double> _x;
    private readonly IReadOnlyList<double> _y;
    private readonly double _yError;
    private readonly Func<double, IReadOnlyList<double>, double> _model;
    private readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? _modelGradient;

    public LeastSquaresWithUniformYError(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        double yError,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelGradient = null,
        double errorDefinitionInSigma = 1)
        : base(parameters, modelGradient != null, errorDefinitionInSigma)
    {
        if (x.Count != y.Count)
            throw new ArgumentException($"{nameof(x)} and {nameof(y)} must have the same length");

        _x = x;
        _y = y;
        _yError = yError;
        _model = model;
        _modelGradient = modelGradient;
    }

    public override double ValueFor(IReadOnlyList<double> parameterValues)
    {
        double sum = 0;
        for (var i = 0; i < _x.Count; i++)
        {
            var residual = ResidualFor(i, parameterValues);
            sum += residual * residual;
        }

        return sum;
    }

    public override IReadOnlyList<double> GradientFor(IReadOnlyList<double> parameterValues)
    {
        var gradientSums = new double[Parameters.Count];
        for (var i = 0; i < _x.Count; i++)
        {
            var factor = 2 * ResidualFor(i, parameterValues);
            var gradients = _modelGradient!(_x[i], parameterValues);
            for (var j = 0; j < Parameters.Count; j++)
                gradientSums[j] -= factor * gradients[j] / _yError;
        }

        return gradientSums;
    }

    private double ResidualFor(int i, IReadOnlyList<double> parameterValues) =>
        (_y[i] - _model(_x[i], parameterValues)) / _yError;
}