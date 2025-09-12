using minuit2.net.CostFunctions;
using static minuit2.net.ParameterMappingGuard;

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
        return new MinimizationResult(minimum, costFunction, true, true);
    }

    private static void Update(FunctionMinimum minimum, ICostFunction costFunction, Strategy strategy)
    {
        using var cost = new CostFunctionAdapter(costFunction);
        using var hesse = new MnHesseWrap(strategy.AsMnStrategy());
        hesse.Update(minimum, cost);
    }
}