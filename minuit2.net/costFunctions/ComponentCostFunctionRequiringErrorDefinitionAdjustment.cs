namespace minuit2.net.costFunctions;

internal class ComponentCostFunctionRequiringErrorDefinitionAdjustment(
    ICostFunctionRequiringErrorDefinitionAdjustment inner,
    IList<string> compositeParameters)
    : ComponentCostFunction(inner, compositeParameters), ICostFunctionRequiringErrorDefinitionAdjustment
{
    private string[] Belonging(IList<string> variables) => variables.Where(var => Parameters.Contains(var)).ToArray();

    public ICostFunctionRequiringErrorDefinitionAdjustment WithErrorDefinitionAdjustedBasedOn(IList<double> parameterValues, IList<string> variables) =>
        new ComponentCostFunctionRequiringErrorDefinitionAdjustment(inner.WithErrorDefinitionAdjustedBasedOn(Belonging(parameterValues), Belonging(variables)), CompositeParameters);
}