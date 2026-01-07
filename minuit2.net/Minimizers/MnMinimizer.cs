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
        ParameterValidation.EnsureUniqueMappingBetween(
            costFunction.Parameters, 
            parameterConfigurations.Select(p => p.Name).ToArray(),
            "parameter configurations", 
            "minimization");
        
        var orderedParameterConfigurations = parameterConfigurations.ExtractInOrder(costFunction.Parameters).ToArray();

        CostFunctionValidation.EnsureValidDerivativeSizes(
            costFunction, 
            orderedParameterConfigurations.Select(p => p.Value).ToArray());

        using var cost = new CostFunctionAdapter(costFunction, cancellationToken);
        using var parameterState = orderedParameterConfigurations.AsState();
        
        minimizerConfiguration ??= new MinimizerConfiguration();
        using var strategy = minimizerConfiguration.Strategy.AsMnStrategy();
        var maximumFunctionCalls = minimizerConfiguration.MaximumFunctionCalls;
        var tolerance = minimizerConfiguration.Tolerance;

        var result = MnMinimize(cost, parameterState, strategy, maximumFunctionCalls, tolerance);

        if (cost.Exceptions.TryDequeue(out var exception))
            return exception is MinimizationAbort abort
                ? new AbortedMinimizationResult(abort, costFunction, parameterState.ExtractVariablesFrom(costFunction.Parameters))
                : throw exception;

        return result.Success 
            ? new MinimizationResult(FunctionMinimumExtensions.Copy(result.FunctionMinimum()), costFunction)
            : throw new CppException();
    }

    protected abstract RunResult MnMinimize(
        FCNWrap costFunction, 
        MnUserParameterState parameterState, 
        MnStrategy strategy, 
        uint maximumFunctionCalls, 
        double tolerance);
}