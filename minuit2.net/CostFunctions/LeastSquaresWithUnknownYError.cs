namespace minuit2.net.CostFunctions;

internal class LeastSquaresWithUnknownYError : LeastSquaresWithUniformYError
{
    private readonly double _errorDefinitionInSigma;

    public LeastSquaresWithUnknownYError(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelGradient = null,
        double errorDefinitionInSigma = 1,
        double errorDefinitionScaling = 1) 
        : base(x, y, 1, parameters, model, modelGradient, errorDefinitionInSigma)
    {
        _errorDefinitionInSigma = errorDefinitionInSigma;
        ErrorDefinition = LeastSquares.ErrorDefinitionFor(errorDefinitionInSigma) * errorDefinitionScaling;
    }

    public override ICostFunction WithErrorDefinitionRecalculatedBasedOnValid(IMinimizationResult result)
    {
        // Auto-adjust the error definition such that a re-evaluation -- e.g., by a subsequent minimization or accurate
        // covariance computation (Hesse algorithm) -- yields the same parameter covariances that would be obtained
        // from a minimization using an error definition of 1, and "perfect y-errors" resulting in a reduced chi-squared
        // value of 1.
        
        // Yet in contrast to adjusting/estimating the y-errors, by adjusting the error definition, the cost value
        // (chi-squared value) itself won't be affected. This means that the resulting reduced chi-squared should
        // approximate the variance of the noise overlying the data.
        
        // This is equivalent to the default behavior in lmfit:
        // https://lmfit.github.io/lmfit-py/fitting.html#uncertainties-in-variable-parameters-and-their-correlations

        var resultParameters = result.Parameters.ToList();
        var parameterValues = Parameters.Select(p => result.ParameterValues[resultParameters.IndexOf(p)]).ToArray();
        var numberOfVariables = Parameters.Count(p => result.Variables.Contains(p));
        
        var degreesOfFreedom = X.Count - numberOfVariables;
        var reducedChiSquared = ValueFor(parameterValues) / degreesOfFreedom;
        return new LeastSquaresWithUnknownYError(
            X,
            Y,
            Parameters,
            Model,
            ModelGradient,
            _errorDefinitionInSigma,
            reducedChiSquared);
    }
}