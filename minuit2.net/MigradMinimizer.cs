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
        var wrappedCostFunction = new CostFunctionWrap(costFunction);
        var parameterState = parameterConfigurations.OrderedBy(costFunction.Parameters).AsState();
        var strategy = new MnStrategy((uint)configuration.Strategy);
        var migrad = new MnMigradWrap(wrappedCostFunction, parameterState, strategy);
        var hesse = new MnHesseWrap(strategy);
        
        var minimum = migrad.Run(configuration.MaximumFunctionCalls, configuration.Tolerance);
        var result = new MinimizationResult(minimum, costFunction);
        if (!costFunction.RequiresErrorDefinitionAutoScaling) return result;
        
        costFunction.AutoScaleErrorDefinitionBasedOn(result.ParameterValues.ToList(), result.Variables.ToList());
        hesse.Update(minimum, wrappedCostFunction);
        return new MinimizationResult(minimum, costFunction);
    }

    private static void ThrowIfParametersAreNotMatchingBetween(ICostFunction costFunction,
        IReadOnlyCollection<ParameterConfiguration> parameterConfigurations)
    {
        if (parameterConfigurations.AreNotMatching(costFunction.Parameters))
            throw new ArgumentException("The given parameter configurations do not match the parameter names defined " +
                                        $"by the cost function: {costFunction.Parameters}");
    }
}