namespace minuit2.net.CostFunctions;

internal class LeastSquaresWithUniformYError(
    IReadOnlyList<double> x,
    IReadOnlyList<double> y,
    double yError,
    IReadOnlyList<string> parameters,
    Func<double, IReadOnlyList<double>, double> model,
    Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelGradient,
    double errorDefinitionInSigma)
    : LeastSquaresBase(x, y, _ => yError, parameters, model, modelGradient, errorDefinitionInSigma);