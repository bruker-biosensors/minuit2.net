using minuit2.net.costFunctions;
using static minuit2.net.MinimizationExitCondition;

namespace minuit2.net;

public static class MigradMinimizer
{
    public static IMinimizationResult Minimize(
        ICostFunction costFunction, 
        IReadOnlyCollection<ParameterConfiguration> parameterConfigurations,
        MigradMinimizerConfiguration? minimizerConfiguration = null, 
        CancellationToken cancellationToken = default)
    {
        ThrowIfParametersAreNotMatchingBetween(costFunction, parameterConfigurations);
        
        minimizerConfiguration ??= new MigradMinimizerConfiguration();
        var result = CoreMinimize(costFunction, parameterConfigurations, minimizerConfiguration, cancellationToken);
        
        if (result.ExitCondition == ManuallyStopped) return result;
        if (costFunction is not ICostFunctionRequiringErrorDefinitionAdjustment cost) return result;

        var adjustedCostFunction = cost.WithAutoScaledErrorDefinitionBasedOn(result.ParameterValues.ToList(), result.Variables.ToList());
        HesseErrorCalculator.UpdateParameterCovariances(result, adjustedCostFunction, minimizerConfiguration.Strategy);
        return result;
    }
    
    private static void ThrowIfParametersAreNotMatchingBetween(
        ICostFunction costFunction,
        IReadOnlyCollection<ParameterConfiguration> parameterConfigurations)
    {
        if (parameterConfigurations.AreNotMatching(costFunction.Parameters))
            throw new ArgumentException("The given parameter configurations do not match the parameter names defined " +
                                        $"by the cost function: {costFunction.Parameters}");
    }

    private static IMinimizationResult CoreMinimize(
        ICostFunction costFunction,
        IReadOnlyCollection<ParameterConfiguration> parameterConfigurations,
        MigradMinimizerConfiguration minimizerConfiguration, 
        CancellationToken cancellationToken)
    {
        using var cost = new CostFunctionAdapter(costFunction, cancellationToken);
        using var parameterState = parameterConfigurations.OrderedBy(costFunction.Parameters).AsState();
        using var migrad = new MnMigradWrap(cost, parameterState, minimizerConfiguration.Strategy.AsMnStrategy());
        
        try
        {
            var minimum = migrad.Run(minimizerConfiguration.MaximumFunctionCalls, minimizerConfiguration.Tolerance);
            return new MinimizationResult(minimum, costFunction);
        }
        catch (OperationCanceledException)
        {
            return new CancelledMinimizationResult();
        }
    }
}