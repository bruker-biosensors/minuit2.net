namespace minuit2.net.CostFunctions;

public interface ICostFunctionRequiringErrorDefinitionAdjustment : ICostFunction
{
    ICostFunctionRequiringErrorDefinitionAdjustment WithErrorDefinitionAdjustedBasedOn(
        IReadOnlyList<double> parameterValues, 
        IReadOnlyList<string> variables);
}