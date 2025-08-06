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
        using var cost = new CostFunctionAdapter(costFunction);
        using var hesse = new MnHesseWrap(strategy.AsMnStrategy());
        hesse.Update(minimum, cost);
    }
}