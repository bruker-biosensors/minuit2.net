namespace minuit2.net.CostFunctions;

internal class LeastSquares : LeastSquaresBase
{
    public LeastSquares(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        IReadOnlyList<double> yError,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelGradient,
        double errorDefinitionInSigma)
        : base(x, y, i => yError[i], parameters, model, modelGradient, errorDefinitionInSigma)
    {
        if (y.Count != yError.Count)
            throw new ArgumentException($"{nameof(y)} and {nameof(yError)} must have the same length");
    }
}