namespace minuit2.net.costFunctions;

public static class CostFunction
{
    public static ICostFunction Sum(params ICostFunction[] components)
    {
        if (components.Any(c => c is ICostFunctionRequiringErrorDefinitionAdjustment))
            return new CostFunctionSumRequiringErrorDefinitionAdjustment(components);
        
        return new CostFunctionSum(components);
    }
    
    internal static ICostFunction Component(ICostFunction costFunction, IList<string> parameters)
    {
        if (costFunction is ICostFunctionRequiringErrorDefinitionAdjustment cost)
            return new ComponentCostFunctionRequiringErrorDefinitionAdjustment(cost, parameters);
        
        return new ComponentCostFunction(costFunction, parameters);
    }
}