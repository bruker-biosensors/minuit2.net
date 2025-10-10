namespace minuit2.net.CostFunctions;

internal class ComponentCostFunctionRequiringErrorDefinitionAdjustment(
    ICostFunctionRequiringErrorDefinitionAdjustment inner,
    IList<string> compositeParameters)
    : ComponentCostFunction(inner, compositeParameters), ICostFunctionRequiringErrorDefinitionAdjustment
{
    private string[] Belonging(IReadOnlyList<string> variables) => variables.Where(x => Parameters.Contains(x)).ToArray();

    public ICostFunctionRequiringErrorDefinitionAdjustment WithErrorDefinitionAdjustedBasedOn(
        IReadOnlyList<double> parameterValues,
        IReadOnlyList<string> variables)
    {
        return new ComponentCostFunctionRequiringErrorDefinitionAdjustment(
            inner.WithErrorDefinitionAdjustedBasedOn(Belonging(parameterValues), Belonging(variables)),
            CompositeParameters);
    }
}