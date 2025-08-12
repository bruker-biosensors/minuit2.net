namespace minuit2.net.CostFunctions;

public static class CostFunction
{
    public static ICostFunction LeastSquares(
        IList<double> x,
        IList<double> y,
        IList<string> parameters,
        Func<double, IList<double>, double> model,
        Func<double, IList<double>, IList<double>>? modelGradient = null, 
        double errorDefinitionInSigma = 1)
    {
        return new LeastSquaresWithUnknownYError(x, y, parameters, model, modelGradient, errorDefinitionInSigma);
    }
    
    public static ICostFunction LeastSquares(
        IList<double> x,
        IList<double> y,
        double yError,
        IList<string> parameters,
        Func<double, IList<double>, double> model,
        Func<double, IList<double>, IList<double>>? modelGradient = null, 
        double errorDefinitionInSigma = 1)
    {
        return new LeastSquaresWithUniformYError(x, y, yError, parameters, model, modelGradient, errorDefinitionInSigma);
    }
    
    public static ICostFunction LeastSquares(
        IList<double> x,
        IList<double> y,
        IList<double> yErrors,
        IList<string> parameters,
        Func<double, IList<double>, double> model,
        Func<double, IList<double>, IList<double>>? modelGradient = null, 
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
    
    internal static ICostFunction Component(ICostFunction costFunction, IList<string> parameters)
    {
        return costFunction is ICostFunctionRequiringErrorDefinitionAdjustment cost
            ? new ComponentCostFunctionRequiringErrorDefinitionAdjustment(cost, parameters)
            : new ComponentCostFunction(costFunction, parameters);
    }
}