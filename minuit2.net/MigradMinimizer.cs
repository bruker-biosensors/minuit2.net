using minuit2.net.wrap;

namespace minuit2.net;

public static class MigradMinimizer
{
    public static MinimizationResult Minimize(
        ICostFunction costFunction, 
        IReadOnlyCollection<ParameterConfiguration> parameterConfigurations,
        MigradMinimizerConfiguration? minimizerConfiguration = null)
    {
        ThrowIfParametersAreNotMatchingBetween(costFunction, parameterConfigurations);
        
        minimizerConfiguration ??= new MigradMinimizerConfiguration();
        var result = CoreMinimize(costFunction, parameterConfigurations, minimizerConfiguration);
        if (!costFunction.RequiresErrorDefinitionAutoScaling) return result;
        
        costFunction.AutoScaleErrorDefinitionBasedOn(result.ParameterValues.ToList(), result.Variables.ToList());
        return HesseErrorCalculator.Update(result, costFunction, minimizerConfiguration.Strategy);
    }
    
    private static void ThrowIfParametersAreNotMatchingBetween(ICostFunction costFunction,
        IReadOnlyCollection<ParameterConfiguration> parameterConfigurations)
    {
        if (parameterConfigurations.AreNotMatching(costFunction.Parameters))
            throw new ArgumentException("The given parameter configurations do not match the parameter names defined " +
                                        $"by the cost function: {costFunction.Parameters}");
    }

    private static MinimizationResult CoreMinimize(
        ICostFunction costFunction,
        IReadOnlyCollection<ParameterConfiguration> parameterConfigurations, 
        MigradMinimizerConfiguration minimizerConfiguration)
    {
        var cost = new CostFunctionWrap(costFunction);
        var parameterState = parameterConfigurations.OrderedBy(costFunction.Parameters).AsState();
        var migrad = new MnMigradWrap(cost, parameterState, minimizerConfiguration.Strategy.AsMnStrategy());

        var minimum = migrad.Run(minimizerConfiguration.MaximumFunctionCalls, minimizerConfiguration.Tolerance);
        return new MinimizationResult(minimum, costFunction);
    }
}