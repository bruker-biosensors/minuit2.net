using minuit2.net;
using minuit2.net.CostFunctions;

namespace minuit2.UnitTests.TestUtilities;

internal static class CostFunctionExtensions
{
    public static double ValueFor(this ICostFunction costFunction, IReadOnlyCollection<ParameterConfiguration> parameters)
    {
        var orderedParameterValues = parameters
            .OrderBy(p => costFunction.Parameters.IndexOf(p.Name))
            .Select(p => p.Value);
        return costFunction.ValueFor(orderedParameterValues);
    }

    public static double ValueFor(this ICostFunction costFunction, IEnumerable<double> parameterValues) =>
        costFunction.ValueFor(parameterValues.ToArray());

    public static ICostFunction WithAutoCancellation(
        this ICostFunction costFunction, 
        CancellationTokenSource cancellationTokenSource, 
        int numberOfFunctionCallsBeforeCancellation)
    {
        return new CostFunctionWithAutoCancellation(
            costFunction, 
            cancellationTokenSource, 
            numberOfFunctionCallsBeforeCancellation);
    }


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

internal class CostFunctionWithAutoCancellation(
    ICostFunction wrapped,
    CancellationTokenSource cancellationTokenSource,
    int numberOfFunctionCallsBeforeCancellation)
    : ICostFunction
{
    private int _numberOfFunctionCalls;

    public IList<string> Parameters => wrapped.Parameters;
    public bool HasGradient => wrapped.HasGradient;
    public double ErrorDefinition => wrapped.ErrorDefinition;
    public double ValueFor(IList<double> parameterValues)
    {
        _numberOfFunctionCalls++;
        if (_numberOfFunctionCalls >= numberOfFunctionCallsBeforeCancellation)  // >= because cancellation is checked before this method call
            cancellationTokenSource.Cancel();
        
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