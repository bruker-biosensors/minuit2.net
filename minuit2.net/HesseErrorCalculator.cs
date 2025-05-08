using minuit2.net.wrap;

namespace minuit2.net;

public static class HesseErrorCalculator
{
    public static void UpdateParameterCovariances(
        IMinimizationResult result, 
        ICostFunction costFunction, 
        Strategy strategy = Strategy.Balanced)
    {
        if (result is not MinimizationResult minimizationResult) return;
        
        var minimum = minimizationResult.Minimum;
        Update(minimum, costFunction, strategy);
        minimizationResult.UpdateParameterCovariancesWith(minimum);
    }

    private static void Update(FunctionMinimum minimum, ICostFunction costFunction, Strategy strategy)
    {
        var cost = new CostFunctionWrap(costFunction);
        var hesse = new MnHesseWrap(strategy.AsMnStrategy());
        hesse.Update(minimum, cost);
    }
}