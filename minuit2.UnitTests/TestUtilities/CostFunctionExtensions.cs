using minuit2.net.CostFunctions;

namespace minuit2.UnitTests.TestUtilities;

internal static class CostFunctionExtensions
{
    public static ICostFunction ListeningToResetEvent(this ICostFunction costFunction, ManualResetEvent resetEvent) =>
        new CostFunctionListeningToResetEvent(costFunction, resetEvent);
    
    public static ICostFunction WithValueSwitchingTo(
        this ICostFunction costFunction, 
        Func<IList<double>, double> valueOverride, 
        int numberOfValueCallsBeforeSwitching = 10)
    {
        return new SwitchingCostFunction(costFunction, valueOverride, null, numberOfValueCallsBeforeSwitching);
    }
    
    public static ICostFunction WithGradientSwitchingTo(
        this ICostFunction costFunction, 
        Func<IList<double>, IList<double>> gradientOverride, 
        int numberOfValueCallsBeforeSwitching = 10)
    {
        return new SwitchingCostFunction(costFunction, null, gradientOverride, numberOfValueCallsBeforeSwitching);
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

internal class SwitchingCostFunction(
    ICostFunction wrapped, 
    Func<IList<double>, double>? valueOverride,
    Func<IList<double>, IList<double>>? gradientOverride,
    int numberOfValueCallsBeforeSwitching)
    : ICostFunction
{
    private int _numberOfValueCalls;
    private bool HasSwitched => _numberOfValueCalls > numberOfValueCallsBeforeSwitching;

    public IList<string> Parameters => wrapped.Parameters;
    public bool HasGradient => wrapped.HasGradient;
    public double ErrorDefinition => wrapped.ErrorDefinition;
    
    public double ValueFor(IList<double> parameterValues)
    {
        _numberOfValueCalls++;
        return HasSwitched && valueOverride != null 
            ? valueOverride(parameterValues) 
            : wrapped.ValueFor(parameterValues);
    }
    
    public IList<double> GradientFor(IList<double> parameterValues) => HasSwitched && gradientOverride != null 
        ? gradientOverride(parameterValues) 
        : wrapped.GradientFor(parameterValues);
}