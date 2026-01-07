using minuit2.net.CostFunctions;

namespace minuit2.net;

public static class HesseErrorCalculator
{
    public static IMinimizationResult Refine(
        IMinimizationResult result,
        ICostFunction costFunction,
        Strategy strategy = Strategy.Balanced,
        CancellationToken cancellationToken = default)
    {
        ParameterValidation.EnsureUniqueMappingBetween(
            costFunction.Parameters,
            result.Parameters,
            "minimization result",
            "error refinement");

        if (result is not MinimizationResult minimizationResult) return result;

        using var hesse = new MnHesseWrap(strategy.AsMnStrategy());
        using var cost = new CostFunctionAdapter(costFunction, cancellationToken);
        
        var minimum = FunctionMinimumExtensions.Copy(minimizationResult.Minimum);
        var success = hesse.Update(minimum, cost);

        if (cost.Exceptions.TryDequeue(out var exception))
            return exception is MinimizationAbort abort
                ? new AbortedMinimizationResult(abort, costFunction, result.Variables, result.NumberOfFunctionCalls)
                : throw exception;
        
        return success
            ? new MinimizationResult(minimum, costFunction)
            : throw new CppException();
    }
}