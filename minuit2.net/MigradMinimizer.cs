using minuit2.net.wrap;
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
        if (!costFunction.RequiresErrorDefinitionAutoScaling || result.ExitCondition == ManuallyStopped) return result;
        
        costFunction.AutoScaleErrorDefinitionBasedOn(result.ParameterValues.ToList(), result.Variables.ToList());
        return HesseErrorCalculator.Update(result, costFunction, minimizerConfiguration.Strategy);
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
        var cost = new CostFunctionWrap(costFunction, cancellationToken);
        var parameterState = parameterConfigurations.OrderedBy(costFunction.Parameters).AsState();
        var migrad = new MnMigradWrap(cost, parameterState, minimizerConfiguration.Strategy.AsMnStrategy());
        
        try
        {
            var minimum = migrad.Run(minimizerConfiguration.MaximumFunctionCalls, minimizerConfiguration.Tolerance);
            return new MinimizationResult(minimum, costFunction);
        }
        catch (OperationCanceledException)
        {
            return new StoppedMinimizationResult();
        }
    }
}