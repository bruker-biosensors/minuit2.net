using minuit2.net.wrap;

namespace minuit2.net;

public static class HesseErrorCalculator
{
    public static MinimizationResult Update(
        MinimizationResult result, 
        ICostFunction costFunction, 
        MinimizationStrategy strategy = MinimizationStrategy.Balanced)
    {
        var minimum = result.FunctionMinimum;
        Update(minimum, costFunction, strategy);
        return new MinimizationResult(minimum, costFunction);
    }

    private static void Update(FunctionMinimum minimum, ICostFunction costFunction, MinimizationStrategy strategy)
    {
        var cost = new CostFunctionWrap(costFunction);
        var hesse = new MnHesseWrap(strategy.ToMnStrategy());
        hesse.Update(minimum, cost);
    }
}