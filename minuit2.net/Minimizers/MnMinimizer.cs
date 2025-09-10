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
        ThrowIfNoUniqueMappingBetween(costFunction.Parameters, parameterConfigurations);

        using var cost = new CostFunctionAdapter(costFunction, cancellationToken);
        using var parameterState = parameterConfigurations.ExtractInOrder(costFunction.Parameters).AsState();
        
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

    private static void ThrowIfNoUniqueMappingBetween(
        IList<string> costFunctionParameters,
        IReadOnlyCollection<ParameterConfiguration> parameterConfigurations)
    {
        var exceptions = new List<Exception>();
        foreach (var name in costFunctionParameters)
        {
            var requiresUniqueConfig = $"Cost function parameter '{name}' requires a unique configuration";
            var numberOfConfigs = parameterConfigurations.Count(p => p.Name == name);
            if (numberOfConfigs == 0)
                exceptions.Add(new ArgumentException($"{requiresUniqueConfig}, but did not receive one."));
            if (numberOfConfigs > 1) 
                exceptions.Add(new ArgumentException($"{requiresUniqueConfig}, but received {numberOfConfigs}."));
        }

        if (exceptions.Count == 1)
            throw exceptions.Single();
        if (exceptions.Count > 1) 
            throw new AggregateException(exceptions);
    }

    protected abstract FunctionMinimum MnMinimize(
        FCNWrap costFunction, 
        MnUserParameterState parameterState, 
        MnStrategy strategy, 
        uint maximumFunctionCalls, 
        double tolerance);
}