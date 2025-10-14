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

        switch (hesse.Update(minimum, cost))
        {
            case MinimizationRunner.RunnerResult.Cancelled:
                return new AbortedMinimizationResult(
                    cost.AbortReason ?? new MinimizationAbort(MinimizationExitCondition.None, [], 0), costFunction,
                    result.Variables, result.NumberOfFunctionCalls);
            case MinimizationRunner.RunnerResult.Error:
                throw new CostFunctionException(hesse.GetErrorMessage());
            case MinimizationRunner.RunnerResult.Success:
                return new MinimizationResult(minimum, costFunction);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
