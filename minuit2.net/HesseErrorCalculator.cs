using minuit2.net.CostFunctions;
using minuit2.net.Exceptions;
using static minuit2.net.MinimizationExitCondition;
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
        var parameterState = minimum.UserState();

        try
        {
            hesse.Update(minimum, cost);
            return new MinimizationResult(minimum, costFunction);
        }
        catch (NonFiniteCostValueException e)
        {
            return new PrematureMinimizationResult(NonFiniteValue, costFunction, parameterState, e.LastParameterValues);
        }
        catch (NonFiniteCostGradientException e)
        {
            return new PrematureMinimizationResult(NonFiniteGradient, costFunction, parameterState, e.LastParameterValues);
        }
        catch (MinimizationCancelledException e)
        {
            return new PrematureMinimizationResult(ManuallyStopped, costFunction, parameterState, e.LastParameterValues);
        }
    }
}