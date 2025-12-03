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
        
        var minimum = Copy(minimizationResult.Minimum);
        var success = hesse.Update(minimum, cost);

        if (cost.Exceptions.TryDequeue(out var exception))
            return exception is MinimizationAbort abort
                ? new AbortedMinimizationResult(abort, costFunction, result.Variables, result.NumberOfFunctionCalls)
                : throw exception;
        
        return success
            ? new MinimizationResult(minimum, costFunction) 
            : throw new Exception("Cpp exception");
    }

    private static FunctionMinimum Copy(FunctionMinimum minimum)
    {
        var copy = new FunctionMinimum(minimum.Seed(), minimum.Up());
        copy.Add(minimum.State());
        return copy;
    }
}