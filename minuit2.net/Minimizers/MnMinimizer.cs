using minuit2.net.CostFunctions;

namespace minuit2.net.Minimizers;

internal abstract class MnMinimizer : IMinimizer
{
    public IMinimizationResult Minimize(
        ICostFunction costFunction, 
        IReadOnlyCollection<ParameterConfiguration> parameterConfigurations,
        MinimizerConfiguration? minimizerConfiguration = null, 
        CancellationToken cancellationToken = default)
    {
        ThrowIfParametersAreNotMatchingBetween(costFunction, parameterConfigurations);
        
        using var cost = new CostFunctionAdapter(costFunction, cancellationToken);
        using var parameterState = parameterConfigurations.OrderedBy(costFunction.Parameters).AsState();
        
        minimizerConfiguration ??= new MinimizerConfiguration();
        using var strategy = minimizerConfiguration.Strategy.AsMnStrategy();
        var maximumFunctionCalls = minimizerConfiguration.MaximumFunctionCalls;
        var tolerance = minimizerConfiguration.Tolerance;
        
        try
        {
            var minimum = MnMinimize(cost, parameterState, strategy, maximumFunctionCalls, tolerance);
            return new MinimizationResult(minimum, costFunction);
        }
        catch (OperationCanceledException)
        {
            return new CancelledMinimizationResult();
        }
    }
    
    private static void ThrowIfParametersAreNotMatchingBetween(
        ICostFunction costFunction,
        IReadOnlyCollection<ParameterConfiguration> parameterConfigurations)
    {
        if (parameterConfigurations.AreNotMatching(costFunction.Parameters))
            throw new ArgumentException("The given parameter configurations do not match the parameter names defined " +
                                        $"by the cost function: {costFunction.Parameters}");
    }
    
    protected abstract FunctionMinimum MnMinimize(
        FCNWrap function, 
        MnUserParameterState parameterState, 
        MnStrategy strategy, 
        uint maximumFunctionCalls, 
        double tolerance);
}