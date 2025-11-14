namespace minuit2.net.CostFunctions;

public static class CostFunction
{
    public static ICostFunction LeastSquares(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelGradient = null,
        double errorDefinitionInSigma = 1)
    {
        return new LeastSquares(x, y, NoYError, parameters, model, modelGradient, errorDefinitionInSigma, true);
        static double NoYError(int _) => 1;
    }

    public static ICostFunction LeastSquares(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        double yError,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelGradient = null,
        double errorDefinitionInSigma = 1)
    {
        return new LeastSquares(x, y, UniformYError, parameters, model, modelGradient, errorDefinitionInSigma, false);
        double UniformYError(int _) => yError;
    }

    public static ICostFunction LeastSquares(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        IReadOnlyList<double> yError,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelGradient = null,
        double errorDefinitionInSigma = 1)
    {
        return y.Count == yError.Count
            ? new LeastSquares(x, y, IndividualYError, parameters, model, modelGradient, errorDefinitionInSigma, false)
            : throw new ArgumentException($"{nameof(y)} and {nameof(yError)} must have the same length");

        double IndividualYError(int index) => yError[index];
    }

    public static ICostFunction Sum(params ICostFunction[] components) => new CostFunctionSum(components);
}