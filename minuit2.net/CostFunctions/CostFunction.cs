using static minuit2.net.CostFunctions.DataPointGuard;

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
        ThrowIfCountMismatchBetween((x, nameof(x)), (y, nameof(y)));
        return new LeastSquares(x, y, _ => 1, parameters, model, modelGradient, errorDefinitionInSigma, true);
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
        ThrowIfCountMismatchBetween((x, nameof(x)), (y, nameof(y)));
        return new LeastSquares(x, y, _ => yError, parameters, model, modelGradient, errorDefinitionInSigma, false);
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
        ThrowIfCountMismatchBetween((x, nameof(x)), (y, nameof(y)), (yError, nameof(yError)));
        return new LeastSquares(x, y, index => yError[index], parameters, model, modelGradient, errorDefinitionInSigma, false);
    }

    public static ICostFunction LeastSquares(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        IReadOnlyList<string> parameters,
        Func<IReadOnlyList<double>, IReadOnlyList<double>, IReadOnlyList<double>> model,
        double errorDefinitionInSigma = 1)
    {
        ThrowIfCountMismatchBetween((x, nameof(x)), (y, nameof(y)));
        return new LeastSquaresWithBatchEvaluationModel(x, y, _ => 1, parameters, model, errorDefinitionInSigma, true);
    }

    public static ICostFunction LeastSquares(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        double yError,
        IReadOnlyList<string> parameters,
        Func<IReadOnlyList<double>, IReadOnlyList<double>, IReadOnlyList<double>> model,
        double errorDefinitionInSigma = 1)
    {
        ThrowIfCountMismatchBetween((x, nameof(x)), (y, nameof(y)));
        return new LeastSquaresWithBatchEvaluationModel(x, y, _ => yError, parameters, model, errorDefinitionInSigma, false);
    }

    public static ICostFunction LeastSquares(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        IReadOnlyList<double> yError,
        IReadOnlyList<string> parameters,
        Func<IReadOnlyList<double>, IReadOnlyList<double>, IReadOnlyList<double>> model,
        double errorDefinitionInSigma = 1)
    {
        ThrowIfCountMismatchBetween((x, nameof(x)), (y, nameof(y)), (yError, nameof(yError)));
        return new LeastSquaresWithBatchEvaluationModel(x, y, index => yError[index], parameters, model, errorDefinitionInSigma, false);
    }

    public static ICostFunction Sum(params ICostFunction[] components) => new CostFunctionSum(components);
}