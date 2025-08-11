using minuit2.net.costFunctions;

namespace minuit2.UnitTests.TestUtilities;

internal static class CostFunctionExtensions
{
    public static ICostFunction WithScaledErrorDefinition(this ICostFunction costFunction, double errorDefinition)
    {
        if(costFunction is LeastSquaresWithUnknownYError leastSquares)
            return new LeastSquaresWithCustomErrorDefinitionRequiringErrorDefinitionAdjustment(leastSquares, errorDefinition);
        return new LeastSquaresWithCustomErrorDefinition(costFunction, errorDefinition);
    }

    public static ICostFunction ListeningToResetEvent(this ICostFunction costFunction, ManualResetEvent resetEvent) =>
        new CostFunctionListeningToResetEvent(costFunction, resetEvent);
}

file class LeastSquaresWithCustomErrorDefinitionRequiringErrorDefinitionAdjustment(
    LeastSquaresWithUnknownYError wrapped,
    double errorDefinitionScaling)
    : ICostFunctionRequiringErrorDefinitionAdjustment
{
    public IList<string> Parameters => wrapped.Parameters;
    public bool HasGradient => wrapped.HasGradient;
    public double ErrorDefinition { get; } = wrapped.ErrorDefinition * errorDefinitionScaling;
    
    public double ValueFor(IList<double> parameterValues) => wrapped.ValueFor(parameterValues);
    public IList<double> GradientFor(IList<double> parameterValues) => wrapped.GradientFor(parameterValues);

    public ICostFunctionRequiringErrorDefinitionAdjustment WithAutoScaledErrorDefinitionBasedOn(IList<double> parameterValues, IList<string> variables)
    {
        var adjustedWrapped = (LeastSquaresWithUnknownYError)wrapped.WithAutoScaledErrorDefinitionBasedOn(parameterValues, variables);
        return new LeastSquaresWithCustomErrorDefinitionRequiringErrorDefinitionAdjustment(adjustedWrapped, errorDefinitionScaling);
    }
}

file class LeastSquaresWithCustomErrorDefinition(ICostFunction wrapped, double errorDefinitionScaling) : ICostFunction
{
    public IList<string> Parameters => wrapped.Parameters;
    public bool HasGradient => wrapped.HasGradient;
    public double ErrorDefinition { get; } = wrapped.ErrorDefinition * errorDefinitionScaling;
    
    public double ValueFor(IList<double> parameterValues) => wrapped.ValueFor(parameterValues);
    public IList<double> GradientFor(IList<double> parameterValues) => wrapped.GradientFor(parameterValues);
}

internal class CostFunctionListeningToResetEvent(ICostFunction wrapped, ManualResetEvent resetEvent) : ICostFunction
{
    public IList<string> Parameters => wrapped.Parameters;
    public bool HasGradient => wrapped.HasGradient;
    public double ErrorDefinition => wrapped.ErrorDefinition;

    public double ValueFor(IList<double> parameterValues)
    {
        resetEvent.WaitOne();
        return wrapped.ValueFor(parameterValues);
    }

    public IList<double> GradientFor(IList<double> parameterValues) => wrapped.GradientFor(parameterValues);
}