namespace minuit2.net.CostFunctions;

internal abstract class LeastSquaresBase(
    IReadOnlyList<string> parameters,
    bool hasGradient,
    double errorDefinitionInSigma,
    double errorDefinitionScaling = 1)
    : ICostFunction
{
    public IReadOnlyList<string> Parameters { get; } = parameters;
    public bool HasGradient { get; } = hasGradient;
    public double ErrorDefinition { get; } = ErrorDefinitionFor(errorDefinitionInSigma) * errorDefinitionScaling;

    public abstract double ValueFor(IReadOnlyList<double> parameterValues);

    public abstract IReadOnlyList<double> GradientFor(IReadOnlyList<double> parameterValues);

    public virtual ICostFunction WithErrorDefinitionRecalculatedBasedOnValid(IMinimizationResult result) => this;

    // For least squares fits, an error definition of 1 corresponds to 1-sigma parameter errors
    // (4 would correspond to 2-sigma errors, 9 would correspond to 3-sigma errors etc.)
    private static double ErrorDefinitionFor(double errorDefinitionInSigma) =>
        errorDefinitionInSigma * errorDefinitionInSigma;
}