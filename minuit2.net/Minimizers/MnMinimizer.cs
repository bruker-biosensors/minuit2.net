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

    public void Minimize(
        IReadOnlyCollection<double> x,
        IReadOnlyCollection<double> y,
        IReadOnlyCollection<double> err,
        IReadOnlyCollection<ParameterConfiguration> parameterConfigurations,
        MinimizerConfiguration? minimizerConfiguration = null)
    {
        using var parameterState = parameterConfigurations.AsState();
        using var cost = new NativeMinimizationFcn(new VectorDouble(x),new VectorDouble(y),new VectorDouble(err), false);
        minimizerConfiguration ??= new MinimizerConfiguration();
        using var strategy = minimizerConfiguration.Strategy.AsMnStrategy();
        var runner = BuildMinimizer(cost, parameterState, strategy);
        var maximumFunctionCalls = minimizerConfiguration.MaximumFunctionCalls;
        var tolerance = minimizerConfiguration.Tolerance;
        switch (runner.Run(maximumFunctionCalls, tolerance))
        {
            case MinimizationRunner.RunnerResult.Cancelled:
                return;
                // var variables =
                //     parameterState.ExtractVariablesFrom(parameterConfigurations.Select(p => p.Name).ToList());
                // return new AbortedMinimizationResult(
                //     new MinimizationAbort(MinimizationExitCondition.None, [], 0), null,
                //     variables);
            case MinimizationRunner.RunnerResult.Error:
                throw new CostFunctionException(runner.GetErrorMessage());
            case MinimizationRunner.RunnerResult.Success:
                return;
                //return new MinimizationResult(runner.GetFunctionMinimum(), null);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected abstract MinimizationRunner BuildMinimizer(
        FcnFacade costFunction,
        MnUserParameterState parameterState,
        MnStrategy strategy);
}
