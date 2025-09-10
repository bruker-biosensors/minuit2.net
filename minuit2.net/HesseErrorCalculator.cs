using minuit2.net.CostFunctions;

namespace minuit2.net;

public static class HesseErrorCalculator
{
    public static IMinimizationResult Refine(
        IMinimizationResult result, 
        ICostFunction costFunction, 
        Strategy strategy = Strategy.Balanced)
    {
        ThrowIfNoUniqueMappingBetween(costFunction.Parameters, result.Parameters, "minimization result", "error refinement");
        
        if (result is not MinimizationResult minimizationResult) return result;
        
        var minimum = minimizationResult.Minimum;
        Update(minimum, costFunction, strategy);
        return new MinimizationResult(minimum, costFunction);
    }

    private static void ThrowIfNoUniqueMappingBetween(
        IEnumerable<string> costParameters, 
        IReadOnlyCollection<string> otherParameters, 
        string otherInstanceName, 
        string taskName)
    {
        var exceptions = new List<Exception>();
        foreach (var name in costParameters)
        {
            var requiresUnique = $"Cost function parameter '{name}' requires a unique match in the " +
                                 $"{otherInstanceName} to perform {taskName}";
            var numberOfMatches = otherParameters.Count(p => p == name);
            if (numberOfMatches == 0)
                exceptions.Add(new ArgumentException($"{requiresUnique}, but there is none."));
            if (numberOfMatches > 1) 
                exceptions.Add(new ArgumentException($"{requiresUnique}, but there are {numberOfMatches}."));
        }

        if (exceptions.Count == 1)
            throw exceptions.Single();
        if (exceptions.Count > 1) 
            throw new AggregateException(exceptions);
    }

    private static void Update(FunctionMinimum minimum, ICostFunction costFunction, Strategy strategy)
    {
        using var cost = new CostFunctionAdapter(costFunction);
        using var hesse = new MnHesseWrap(strategy.AsMnStrategy());
        hesse.Update(minimum, cost);
    }
}