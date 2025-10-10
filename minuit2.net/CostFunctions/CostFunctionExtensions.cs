namespace minuit2.net.CostFunctions;

public static class CostFunctionExtensions
{
    public static ICostFunction WithErrorDefinitionAdjustedWhereRequiredBasedOn(
        this ICostFunction costFunction, 
        IMinimizationResult result)
    {
        return costFunction is ICostFunctionRequiringErrorDefinitionAdjustment cost
            ? cost.WithErrorDefinitionAdjustedBasedOn(result.ParameterValues.ToList(), result.Variables.ToList())
            : costFunction;
    }
}