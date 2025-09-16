using minuit2.net.CostFunctions;

namespace minuit2.UnitTests.TestUtilities;

internal static class CostFunctionExtensions
{
    public static double ValueFor(this ICostFunction costFunction, IEnumerable<double> parameterValues) =>
        costFunction.ValueFor(parameterValues.ToArray());
    
    public static IList<double> GradientFor(this ICostFunction costFunction, IEnumerable<double> parameterValues) =>
        costFunction.GradientFor(parameterValues.ToArray());
    
    public static ICostFunction ListeningToResetEvent(this ICostFunction costFunction, ManualResetEvent resetEvent) =>
        new CostFunctionListeningToResetEvent(costFunction, resetEvent);
    
    public static ICostFunction WithValueOverride(
        this ICostFunction costFunction, 
        Func<IList<double>, double> valueOverride, 
        int numberOfFunctionCallsBeforeReturningOverride = 10)
    {
        return new CostFunctionReturningOverrides(
            costFunction, 
            valueOverride, 
            null, 
            numberOfFunctionCallsBeforeReturningOverride);
    }
    
    public static ICostFunction WithGradientOverride(
        this ICostFunction costFunction, 
        Func<IList<double>, IList<double>> gradientOverride, 
        int numberOfFunctionCallsBeforeReturningOverride = 10)
    {
        return new CostFunctionReturningOverrides(
            costFunction, 
            null, 
            gradientOverride, 
            numberOfFunctionCallsBeforeReturningOverride);
    }
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

internal class CostFunctionReturningOverrides(
    ICostFunction wrapped, 
    Func<IList<double>, double>? valueOverride,
    Func<IList<double>, IList<double>>? gradientOverride,
    int numberOfFunctionCallsBeforeReturningOverrides)
    : ICostFunction
{
    private int _numberOfFunctionCalls;
    private bool HasSwitched => _numberOfFunctionCalls > numberOfFunctionCallsBeforeReturningOverrides;

    public IList<string> Parameters => wrapped.Parameters;
    public bool HasGradient => wrapped.HasGradient;
    public double ErrorDefinition => wrapped.ErrorDefinition;
    
    public double ValueFor(IList<double> parameterValues)
    {
        _numberOfFunctionCalls++;
        return HasSwitched && valueOverride != null 
            ? valueOverride(parameterValues) 
            : wrapped.ValueFor(parameterValues);
    }
    
    public IList<double> GradientFor(IList<double> parameterValues) => HasSwitched && gradientOverride != null 
        ? gradientOverride(parameterValues) 
        : wrapped.GradientFor(parameterValues);
}