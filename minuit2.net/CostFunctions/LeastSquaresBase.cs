namespace minuit2.net.CostFunctions;

internal abstract class LeastSquaresBase(
    int numberOfDataPoints,
    IReadOnlyList<string> parameters,
    bool hasGradient,
    double errorDefinition)
    : ICostFunction
{
    public IReadOnlyList<string> Parameters { get; } = parameters;
    public bool HasGradient { get; } = hasGradient;
    public double ErrorDefinition { get; } = errorDefinition;
    
    // For least squares fits, an error definition of 1 corresponds to 1-sigma parameter errors
    // (4 would correspond to 2-sigma errors, 9 would correspond to 3-sigma errors etc.)
    protected static double ErrorDefinitionFor(double sigma, double scaling) => sigma * sigma * scaling;
    
    public abstract double ValueFor(IReadOnlyList<double> parameterValues);

    public abstract IReadOnlyList<double> GradientFor(IReadOnlyList<double> parameterValues);

    public ICostFunction WithErrorDefinitionRecalculatedBasedOnValid(IMinimizationResult result)
    {
        // Auto-scale the error definition such that a re-evaluation -- e.g., by a following minimization or accurate
        // covariance computation (Hesse algorithm) -- yields the same parameter covariances that would be obtained
        // from a minimization using an "ideal y-error" resulting in a reduced chi-squared value of 1.

        // By scaling the error definition, the cost value (chi-squared value) itself won't be affected. This means
        // that the resulting reduced chi-squared should approximate the variance of the noise overlying the data.

        // This is equivalent to the default behavior in lmfit:
        // https://lmfit.github.io/lmfit-py/fitting.html#uncertainties-in-variable-parameters-and-their-correlations

        var resultParameters = result.Parameters.ToList();
        var parameterValues = Parameters.Select(p => result.ParameterValues[resultParameters.IndexOf(p)]).ToArray();
        var numberOfVariables = Parameters.Count(p => result.Variables.Contains(p));

        var degreesOfFreedom = numberOfDataPoints - numberOfVariables;
        var reducedChiSquared = ValueFor(parameterValues) / degreesOfFreedom;
        return CopyWith(reducedChiSquared);
    }

    protected abstract ICostFunction CopyWith(double errorDefinitionScaling);
}