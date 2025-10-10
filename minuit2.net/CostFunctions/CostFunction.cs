namespace minuit2.net.CostFunctions;

public static class CostFunction
{
    public static ICostFunction LeastSquares(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelGradient = null,
        double errorDefinitionInSigma = 1)
    {
        return new LeastSquaresWithUnknownYError(x, y, parameters, model, modelGradient, errorDefinitionInSigma);
    }
    
    public static ICostFunction LeastSquares(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        double yError,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelGradient = null,
        double errorDefinitionInSigma = 1)
    {
        return new LeastSquaresWithUniformYError(x, y, yError, parameters, model, modelGradient, errorDefinitionInSigma);
    }
    
    public static ICostFunction LeastSquares(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        IReadOnlyList<double> yErrors,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelGradient = null,
        double errorDefinitionInSigma = 1)
    {
        return new LeastSquares(x, y, yErrors, parameters, model, modelGradient, errorDefinitionInSigma);
    }
    
    public static ICostFunction Sum(params ICostFunction[] components)
    {
        return components.Any(c => c is ICostFunctionRequiringErrorDefinitionAdjustment) 
            ? new CostFunctionSumRequiringErrorDefinitionAdjustment(components) 
            : new CostFunctionSum(components);
    }
    
    internal static ICostFunction Component(ICostFunction costFunction, IList<string> compositeParameters)
    {
        return costFunction is ICostFunctionRequiringErrorDefinitionAdjustment cost
            ? new ComponentCostFunctionRequiringErrorDefinitionAdjustment(cost, compositeParameters)
            : new ComponentCostFunction(costFunction, compositeParameters);
    }
}