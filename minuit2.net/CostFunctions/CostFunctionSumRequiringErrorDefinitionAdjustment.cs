namespace minuit2.net.CostFunctions;

internal class CostFunctionSumRequiringErrorDefinitionAdjustment(params ICostFunction[] components)
    : CostFunctionSum(components), ICostFunctionRequiringErrorDefinitionAdjustment
{
    public ICostFunctionRequiringErrorDefinitionAdjustment WithErrorDefinitionAdjustedBasedOn(
        IReadOnlyList<double> parameterValues, 
        IReadOnlyList<string> variables)
    {
        var adjustedComponents = Components.Select(c => c is ICostFunctionRequiringErrorDefinitionAdjustment adjustable 
            ? adjustable.WithErrorDefinitionAdjustedBasedOn(parameterValues, variables)
            : c).ToArray();
        return new CostFunctionSumRequiringErrorDefinitionAdjustment(adjustedComponents);
    }
}