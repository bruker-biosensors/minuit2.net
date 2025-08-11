using minuit2.net;

namespace minuit2.UnitTests.TestUtilities;

internal static class CostFunctionExtensions
{
    public static ICostFunctionRequiringErrorDefinitionAdjustment WithErrorDefinition(this LeastSquaresWithUniformYError cost, double errorDefinition) =>
        new LeastSquaresWithCustomErrorDefinition(cost, errorDefinition);

    public static ICostFunctionRequiringErrorDefinitionAdjustment ListeningToResetEvent(this ICostFunctionRequiringErrorDefinitionAdjustment cost, ManualResetEvent resetEvent) =>
        new CostFunctionListeningToResetEvent(cost, resetEvent);
}

file class LeastSquaresWithCustomErrorDefinition(LeastSquaresWithUniformYError wrapped, double errorDefinition) : ICostFunctionRequiringErrorDefinitionAdjustment
{
    private readonly double _errorDefinition = errorDefinition;

    public IList<string> Parameters => wrapped.Parameters;
    public bool HasGradient => wrapped.HasGradient;
    public double ErrorDefinition { get; private set; } = errorDefinition;
    public bool RequiresErrorDefinitionAutoScaling => wrapped.RequiresErrorDefinitionAutoScaling;
    
    public double ValueFor(IList<double> parameterValues) => wrapped.ValueFor(parameterValues);
    public IList<double> GradientFor(IList<double> parameterValues) => wrapped.GradientFor(parameterValues);
    public ICostFunctionRequiringErrorDefinitionAdjustment WithAutoScaledErrorDefinitionBasedOn(IList<double> parameterValues, IList<string> variables)
    {
        var wrappedCopy = (LeastSquaresWithUniformYError)wrapped.WithAutoScaledErrorDefinitionBasedOn(parameterValues, variables);
        var errorDefinitionScaling = wrappedCopy.ErrorDefinition / wrapped.ErrorDefinition;
        return new LeastSquaresWithCustomErrorDefinition(wrappedCopy, ErrorDefinition * errorDefinitionScaling);
    }
}

internal class CostFunctionListeningToResetEvent(ICostFunctionRequiringErrorDefinitionAdjustment wrapped, ManualResetEvent resetEvent) : ICostFunctionRequiringErrorDefinitionAdjustment
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

    public ICostFunctionRequiringErrorDefinitionAdjustment WithAutoScaledErrorDefinitionBasedOn(IList<double> parameterValues, IList<string> variables) =>
        new CostFunctionListeningToResetEvent(wrapped.WithAutoScaledErrorDefinitionBasedOn(parameterValues, variables), resetEvent);
}