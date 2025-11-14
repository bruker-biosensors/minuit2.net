namespace minuit2.net.CostFunctions;

internal class LeastSquares : LeastSquaresBase
{
    private readonly IReadOnlyList<double> _yError;

    public LeastSquares(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        IReadOnlyList<double> yError,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelGradient = null,
        double errorDefinitionInSigma = 1)
        : base(x, y, parameters, model, modelGradient, errorDefinitionInSigma)
    {
        if (y.Count != yError.Count)
            throw new ArgumentException($"{nameof(y)} and {nameof(yError)} must have the same length");

        _yError = yError;
    }

    protected override double YErrorFor(int index) => _yError[index];
}