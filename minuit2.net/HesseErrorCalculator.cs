using minuit2.net.wrap;

namespace minuit2.net;

public static class HesseErrorCalculator
{
    public static IMinimizationResult Update(
        IMinimizationResult result, 
        ICostFunction costFunction, 
        Strategy strategy = Strategy.Balanced)
    {
        if (result is not MinimizationResult minimizationResult) return result;
        
        var minimum = minimizationResult.FunctionMinimum;
        Update(minimum, costFunction, strategy);
        return new MinimizationResult(minimum, costFunction, minimizationResult.Tolerance);
    }

    private static void Update(FunctionMinimum minimum, ICostFunction costFunction, Strategy strategy)
    {
        var cost = new CostFunctionWrap(costFunction);
        var hesse = new MnHesseWrap(strategy.AsMnStrategy());
        hesse.Update(minimum, cost);
    }
}