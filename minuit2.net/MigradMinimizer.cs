using minuit2.net.wrap;

namespace minuit2.net;

public static class MigradMinimizer
{
    public static MinimizationResult Minimize(
        ICostFunction costFunction, 
        IReadOnlyCollection<ParameterConfiguration> parameterConfigurations,
        MigradMinimizerConfiguration? configuration = null)
    {
        ThrowIfParametersAreNotMatchingBetween(costFunction, parameterConfigurations);
        
        configuration ??= new MigradMinimizerConfiguration();
        var result = CoreMinimize(costFunction, parameterConfigurations, configuration);
        if (!costFunction.RequiresErrorDefinitionAutoScaling) return result;
        
        costFunction.AutoScaleErrorDefinitionBasedOn(result.ParameterValues.ToList(), result.Variables.ToList());
        return HesseErrorCalculator.Update(result, costFunction, configuration.Strategy);
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
        MigradMinimizerConfiguration configuration)
    {
        var cost = new CostFunctionWrap(costFunction);
        var parameterState = parameterConfigurations.OrderedBy(costFunction.Parameters).AsState();
        var strategy = new MnStrategy((uint)configuration.Strategy);
        var migrad = new MnMigradWrap(cost, parameterState, strategy);

        var minimum = migrad.Run(configuration.MaximumFunctionCalls, configuration.Tolerance);
        return new MinimizationResult(minimum, costFunction);
    }
}