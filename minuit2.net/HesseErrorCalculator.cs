using minuit2.net.wrap;

namespace minuit2.net;

public static class HesseErrorCalculator
{
    public static IMinimizationResult UpdateParameterCovariances(
        IMinimizationResult result, 
        ICostFunction costFunction, 
        Strategy strategy = Strategy.Balanced)
    {
        if (result is not MinimizationResult minimizationResult) return result;
        
        var minimum = minimizationResult.Minimum;
        UpdateParameterCovariances(minimum, costFunction, strategy);
        return new MinimizationResult(minimum, costFunction, minimizationResult.EdmThreshold);
    }

    private static void UpdateParameterCovariances(FunctionMinimum minimum, ICostFunction costFunction, Strategy strategy)
    {
        var cost = new CostFunctionWrap(costFunction);
        var hesse = new MnHesseWrap(strategy.AsMnStrategy());
        hesse.Update(minimum, cost);
    }
}