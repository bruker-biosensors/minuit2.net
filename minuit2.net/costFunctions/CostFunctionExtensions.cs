namespace minuit2.net.costFunctions;

public static class CostFunctionExtensions
{
    public static ICostFunction WithErrorDefinitionAdjustedWhereRequiredBasedOn(
        this ICostFunction costFunction, 
        IMinimizationResult result)
    {
        return costFunction is ICostFunctionRequiringErrorDefinitionAdjustment cost
            ? cost.WithAdjustedErrorDefinitionBasedOn(result.ParameterValues.ToList(), result.Variables.ToList())
            : costFunction;
    }
}