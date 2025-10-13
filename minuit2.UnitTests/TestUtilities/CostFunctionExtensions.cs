using minuit2.net;
using minuit2.net.CostFunctions;

namespace minuit2.UnitTests.TestUtilities;

internal static class CostFunctionExtensions
{
    public static double ValueFor(this ICostFunction costFunction, IReadOnlyCollection<ParameterConfiguration> parameters)
    {
        var orderedParameterValues = costFunction.Parameters.Select(Value);
        return costFunction.ValueFor(orderedParameterValues);

        double Value(string parameterName) => parameters.Single(config => config.Name == parameterName).Value;
    }

    private static double ValueFor(this ICostFunction costFunction, IEnumerable<double> parameterValues) =>
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
        Func<IReadOnlyList<double>, double> valueOverride,
        int numberOfFunctionCallsBeforeReturningOverride = 10)
    {
        return new CostFunctionWithOverrides(
            costFunction, 
            valueOverride, 
            null, 
            numberOfFunctionCallsBeforeReturningOverride);
    }
    
    public static ICostFunction WithGradientOverride(
        this ICostFunction costFunction,
        Func<IReadOnlyList<double>, IReadOnlyList<double>> gradientOverride,
        int numberOfFunctionCallsBeforeReturningOverride = 10)
    {
        return new CostFunctionWithOverrides(
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

    public IReadOnlyList<string> Parameters => wrapped.Parameters;
    public bool HasGradient => wrapped.HasGradient;
    public double ErrorDefinition => wrapped.ErrorDefinition;
    public double ValueFor(IReadOnlyList<double> parameterValues)
    {
        Interlocked.Increment(ref _numberOfFunctionCalls);
        
        if (_numberOfFunctionCalls >= numberOfFunctionCallsBeforeCancellation)
            cancellationTokenSource.Cancel();
        
        return wrapped.ValueFor(parameterValues);
    }

    public IReadOnlyList<double> GradientFor(IReadOnlyList<double> parameterValues) =>
        wrapped.GradientFor(parameterValues);
}

internal class CostFunctionWithOverrides(
    ICostFunction wrapped,
    Func<IReadOnlyList<double>, double>? valueOverride,
    Func<IReadOnlyList<double>, IReadOnlyList<double>>? gradientOverride,
    int numberOfFunctionCallsBeforeReturningOverrides)
    : ICostFunction
{
    private int _numberOfFunctionCalls;
    private bool HasSwitched => _numberOfFunctionCalls > numberOfFunctionCallsBeforeReturningOverrides;

    public IReadOnlyList<string> Parameters => wrapped.Parameters;
    public bool HasGradient => wrapped.HasGradient;
    public double ErrorDefinition => wrapped.ErrorDefinition;
    
    public double ValueFor(IReadOnlyList<double> parameterValues)
    {
        Interlocked.Increment(ref _numberOfFunctionCalls);
        
        return HasSwitched && valueOverride != null 
            ? valueOverride(parameterValues) 
            : wrapped.ValueFor(parameterValues);
    }
    
    public IReadOnlyList<double> GradientFor(IReadOnlyList<double> parameterValues)
    {
        return HasSwitched && gradientOverride != null
            ? gradientOverride(parameterValues)
            : wrapped.GradientFor(parameterValues);
    }
}