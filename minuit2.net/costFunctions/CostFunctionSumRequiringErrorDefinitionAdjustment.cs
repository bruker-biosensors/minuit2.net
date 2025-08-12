namespace minuit2.net.costFunctions;

internal class CostFunctionSumRequiringErrorDefinitionAdjustment(params ICostFunction[] components)
    : CostFunctionSum(components), ICostFunctionRequiringErrorDefinitionAdjustment
{
    public ICostFunctionRequiringErrorDefinitionAdjustment WithErrorDefinitionAdjustedBasedOn(IList<double> parameterValues, IList<string> variables) =>
        new CostFunctionSumRequiringErrorDefinitionAdjustment(
            Components.Select(c => c is ICostFunctionRequiringErrorDefinitionAdjustment adjustable
                ? adjustable.WithErrorDefinitionAdjustedBasedOn(parameterValues, variables) 
                : c).ToArray());
}