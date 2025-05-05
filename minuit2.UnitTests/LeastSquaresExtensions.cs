using minuit2.net;

namespace minuit2.UnitTests;

internal static class LeastSquaresExtensions
{
    public static ICostFunction WithErrorDefinition(this LeastSquares cost, double errorDefinition) =>
        new LeastSquaresWithCustomErrorDefinition(cost, errorDefinition);
}

file class LeastSquaresWithCustomErrorDefinition(LeastSquares wrapped, double errorDefinition) : ICostFunction
{
    private readonly double _errorDefinition = errorDefinition;

    public IList<string> Parameters => wrapped.Parameters;
    public bool HasGradient => wrapped.HasGradient;
    public double ErrorDefinition { get; private set; } = errorDefinition;
    public bool RequiresErrorDefinitionAutoScaling => wrapped.RequiresErrorDefinitionAutoScaling;
    
    public double ValueFor(IList<double> parameterValues) => wrapped.ValueFor(parameterValues);
    public IList<double> GradientFor(IList<double> parameterValues) => wrapped.GradientFor(parameterValues);
    public void AutoScaleErrorDefinitionBasedOn(IList<double> parameterValues, IList<string> variables)
    {
        wrapped.AutoScaleErrorDefinitionBasedOn(parameterValues, variables);
        ErrorDefinition = _errorDefinition * wrapped.ErrorDefinition;  // Works because the wrapped (default) least squares error definition is 1 (before scaling)
    }
    public double AdjustedValueFor(IList<double> parameterValues) => wrapped.AdjustedValueFor(parameterValues);
}