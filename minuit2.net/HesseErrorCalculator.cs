using minuit2.net.CostFunctions;
using static minuit2.net.ParameterMappingGuard;

namespace minuit2.net;

public static class HesseErrorCalculator
{
    public static IMinimizationResult Refine(
        IMinimizationResult result,
        ICostFunction costFunction,
        Strategy strategy = Strategy.Balanced,
        CancellationToken cancellationToken = default)
    {
        ThrowIfNoUniqueMappingBetween(
            costFunction.Parameters,
            result.Parameters,
            "minimization result",
            "error refinement");

        if (result is not MinimizationResult minimizationResult) return result;

        using var hesse = new MnHesseWrap(strategy.AsMnStrategy());
        using var cost = new CostFunctionAdapter(costFunction, cancellationToken);
        var minimum = minimizationResult.Minimum;
        var initialState = minimum.UserState();

        try
        {
            hesse.Update(minimum, cost);
            return new MinimizationResult(minimum, costFunction);
        }
        catch (NonFiniteCostValueException)
        {
            return new PrematureMinimizationResult(MinimizationExitCondition.NonFiniteValue, costFunction, cost, initialState);
        }
        catch (NonFiniteCostGradientException)
        {
            return new PrematureMinimizationResult(MinimizationExitCondition.NonFiniteGradient, costFunction, cost, initialState);
        }
        catch (OperationCanceledException)
        {
            return new CancelledMinimizationResult();
        }
    }
}