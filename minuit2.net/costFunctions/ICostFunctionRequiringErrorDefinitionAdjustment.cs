namespace minuit2.net.costFunctions;

public interface ICostFunctionRequiringErrorDefinitionAdjustment : ICostFunction
{
    ICostFunctionRequiringErrorDefinitionAdjustment WithAdjustedErrorDefinitionBasedOn(IList<double> parameterValues, IList<string> variables);
}