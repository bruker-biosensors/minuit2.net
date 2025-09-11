using minuit2.net.CostFunctions;
using static minuit2.net.ParameterMappingGuard;

namespace minuit2.net.Minimizers;

internal abstract class MnMinimizer : IMinimizer
{
    public IMinimizationResult Minimize(
        ICostFunction costFunction, 
        IReadOnlyCollection<ParameterConfiguration> parameterConfigurations,
        MinimizerConfiguration? minimizerConfiguration = null, 
        CancellationToken cancellationToken = default)
    {
        ThrowIfNoUniqueMappingBetween(
            costFunction.Parameters, 
            parameterConfigurations.Select(p => p.Name).ToArray(),
            "parameter configurations", 
            "minimization");

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

    protected abstract FunctionMinimum MnMinimize(
        FCNWrap costFunction, 
        MnUserParameterState parameterState, 
        MnStrategy strategy, 
        uint maximumFunctionCalls, 
        double tolerance);
}