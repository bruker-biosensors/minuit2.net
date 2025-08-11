namespace minuit2.net;

internal class ComponentCostFunctionRequiringErrorDefinitionAdjustment(
    ICostFunctionRequiringErrorDefinitionAdjustment inner,
    IList<string> compositeParameters)
    : ComponentCostFunction(inner, compositeParameters), ICostFunctionRequiringErrorDefinitionAdjustment
{
    private string[] Belonging(IList<string> variables) => variables.Where(var => Parameters.Contains(var)).ToArray();

    public ICostFunctionRequiringErrorDefinitionAdjustment WithAutoScaledErrorDefinitionBasedOn(IList<double> parameterValues, IList<string> variables) =>
        new ComponentCostFunctionRequiringErrorDefinitionAdjustment(inner.WithAutoScaledErrorDefinitionBasedOn(Belonging(parameterValues), Belonging(variables)), CompositeParameters);
}