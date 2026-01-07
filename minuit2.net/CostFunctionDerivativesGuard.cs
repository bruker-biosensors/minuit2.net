using minuit2.net.CostFunctions;

namespace minuit2.net;

internal static class CostFunctionDerivativesGuard
{
    public static void ThrowIfDerivativesAreOfIncorrectSize(
        ICostFunction costFunction, 
        IReadOnlyList<double> parameterValues)
    {
        var exceptions = new List<Exception>();

        if (costFunction.HasGradient) 
            CheckGradient(costFunction, parameterValues, exceptions);
        if (costFunction.HasHessian) 
            CheckHessian(costFunction, parameterValues, exceptions);
        if (costFunction.HasHessianDiagonal) 
            CheckHessianDiagonal(costFunction, parameterValues, exceptions);
        
        if (exceptions.Count == 1)
            throw exceptions.Single();
        if (exceptions.Count > 1) 
            throw new AggregateException(exceptions);
    }

    private static void CheckGradient(ICostFunction costFunction, IReadOnlyList<double> parameterValues, List<Exception> exceptions)
    {
        var size = costFunction.GradientFor(parameterValues).Count;
        var expectedSize = costFunction.Parameters.Count;
        if (size != expectedSize)
            exceptions.Add(new CostFunctionError($"Invalid gradient size: expected {expectedSize} value(s) " +
                                                 $"(one per parameter), but got {size}."));
    }

    private static void CheckHessian(ICostFunction costFunction, IReadOnlyList<double> parameterValues, List<Exception> exceptions)
    {
        var size = costFunction.HessianFor(parameterValues).Count;
        var expectedSize = costFunction.Parameters.Count * costFunction.Parameters.Count;
        if (size != expectedSize)
            exceptions.Add(new CostFunctionError($"Invalid Hessian size: expected {expectedSize} value(s) " +
                                                 $"(one per parameter pair), but got {size}."));
    }

    private static void CheckHessianDiagonal(ICostFunction costFunction, IReadOnlyList<double> parameterValues,
        List<Exception> exceptions)
    {
        var size = costFunction.HessianDiagonalFor(parameterValues).Count;
        var expectedSize = costFunction.Parameters.Count;
        if (size != expectedSize)
            exceptions.Add(new CostFunctionError($"Invalid Hessian diagonal size: expected {expectedSize} value(s) " +
                                                 $"(one per parameter pair), but got {size}."));
    }
}