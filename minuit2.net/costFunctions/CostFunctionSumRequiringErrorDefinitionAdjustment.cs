namespace minuit2.net.costFunctions;

internal class CostFunctionSumRequiringErrorDefinitionAdjustment(params ICostFunction[] components)
    : CostFunctionSum(components), ICostFunctionRequiringErrorDefinitionAdjustment
{
    public ICostFunctionRequiringErrorDefinitionAdjustment WithAdjustedErrorDefinitionBasedOn(IList<double> parameterValues, IList<string> variables) =>
        new CostFunctionSumRequiringErrorDefinitionAdjustment(
            Components.Select(c => c is ICostFunctionRequiringErrorDefinitionAdjustment adjustable
                ? adjustable.WithAdjustedErrorDefinitionBasedOn(parameterValues, variables) 
                : c).ToArray());
}