namespace minuit2.net.CostFunctions;

public interface ICostFunctionRequiringErrorDefinitionAdjustment : ICostFunction
{
    ICostFunctionRequiringErrorDefinitionAdjustment WithErrorDefinitionAdjustedBasedOn(IList<double> parameterValues, IList<string> variables);
}