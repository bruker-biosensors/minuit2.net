using minuit2.net;

namespace minuit2.UnitTests.TestUtilities;

internal static class CostFunctionExtensions
{
    public static ICostFunction WithErrorDefinition(this LeastSquares cost, double errorDefinition) =>
        new LeastSquaresWithCustomErrorDefinition(cost, errorDefinition);

    public static ICostFunction ListeningToResetEvent(this ICostFunction cost, ManualResetEvent resetEvent) =>
        new CostFunctionListeningToResetEvent(cost, resetEvent);
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
}

internal class CostFunctionListeningToResetEvent(ICostFunction wrapped, ManualResetEvent resetEvent) : ICostFunction
{
    public IList<string> Parameters => wrapped.Parameters;
    public bool HasGradient => wrapped.HasGradient;
    public double ErrorDefinition => wrapped.ErrorDefinition;
    public bool RequiresErrorDefinitionAutoScaling => wrapped.RequiresErrorDefinitionAutoScaling;

    public double ValueFor(IList<double> parameterValues)
    {
        resetEvent.WaitOne();
        return wrapped.ValueFor(parameterValues);
    }

    public IList<double> GradientFor(IList<double> parameterValues) => wrapped.GradientFor(parameterValues);

    public void AutoScaleErrorDefinitionBasedOn(IList<double> parameterValues, IList<string> variables) =>
        wrapped.AutoScaleErrorDefinitionBasedOn(parameterValues, variables);
}