using minuit2.net.CostFunctions;

namespace minuit2.net;

internal static class CostFunctionValidation
{
    public static void EnsureValidDerivativeSizes(
        ICostFunction costFunction, 
        IReadOnlyList<double> parameterValues)
    {
        var exceptions = new List<Exception>();

        if (costFunction.HasGradient) 
            EnsureValidGradientSize(costFunction, parameterValues, exceptions);
        if (costFunction.HasHessian) 
            EnsureValidHessianSize(costFunction, parameterValues, exceptions);
        if (costFunction.HasHessianDiagonal) 
            EnsureValidHessianDiagonalSize(costFunction, parameterValues, exceptions);
        
        if (exceptions.Count == 1)
            throw exceptions.Single();
        if (exceptions.Count > 1) 
            throw new AggregateException(exceptions);
    }

    private static void EnsureValidGradientSize(
        ICostFunction costFunction,
        IReadOnlyList<double> parameterValues,
        List<Exception> exceptions)
    {
        var size = costFunction.GradientFor(parameterValues).Count;
        var expectedSize = costFunction.Parameters.Count;
        if (size != expectedSize)
            exceptions.Add(new InvalidCostFunctionException(
                $"Invalid gradient size: expected {expectedSize} value(s) (one per parameter), but got {size}."));
    }

    private static void EnsureValidHessianSize(
        ICostFunction costFunction,
        IReadOnlyList<double> parameterValues,
        List<Exception> exceptions)
    {
        var size = costFunction.HessianFor(parameterValues).Count;
        var expectedSize = costFunction.Parameters.Count * costFunction.Parameters.Count;
        if (size != expectedSize)
            exceptions.Add(new InvalidCostFunctionException(
                $"Invalid Hessian size: expected {expectedSize} value(s) (one per parameter pair), but got {size}."));
    }

    private static void EnsureValidHessianDiagonalSize(
        ICostFunction costFunction,
        IReadOnlyList<double> parameterValues,
        List<Exception> exceptions)
    {
        var size = costFunction.HessianDiagonalFor(parameterValues).Count;
        var expectedSize = costFunction.Parameters.Count;
        if (size != expectedSize)
            exceptions.Add(new InvalidCostFunctionException(
                $"Invalid Hessian diagonal size: expected {expectedSize} value(s) (one per parameter), but got {size}."));
    }
}