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

        var runner = BuildMinimizer(cost, parameterState, strategy);
        switch (runner.Run(maximumFunctionCalls, tolerance))
        {
            case MinimizationRunner.RunnerResult.Cancelled:
                var variables = parameterState.ExtractVariablesFrom(costFunction.Parameters);
                return new AbortedMinimizationResult(
                    cost.AbortReason ?? new MinimizationAbort(MinimizationExitCondition.None, [], 0), costFunction,
                    variables);
            case MinimizationRunner.RunnerResult.Error:
                throw new CostFunctionException(runner.GetErrorMessage());
            case MinimizationRunner.RunnerResult.Success:
                return new MinimizationResult(runner.GetFunctionMinimum(), costFunction);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected abstract MinimizationRunner BuildMinimizer(
        FCNWrap costFunction,
        MnUserParameterState parameterState,
        MnStrategy strategy);
}
