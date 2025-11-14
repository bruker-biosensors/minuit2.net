namespace minuit2.net.CostFunctions;

internal class LeastSquaresWithUnknownYError(
    IReadOnlyList<double> x,
    IReadOnlyList<double> y,
    IReadOnlyList<string> parameters,
    Func<double, IReadOnlyList<double>, double> model,
    Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelGradient,
    double errorDefinitionInSigma,
    double errorDefinitionScaling = 1)
    : LeastSquaresBase(x, y, _ => 1, parameters, model, modelGradient, errorDefinitionInSigma, errorDefinitionScaling, true);