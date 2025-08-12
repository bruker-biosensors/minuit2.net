namespace minuit2.net.costFunctions;

public interface ICostFunctionRequiringErrorDefinitionAdjustment : ICostFunction
{
    ICostFunctionRequiringErrorDefinitionAdjustment WithErrorDefinitionAdjustedBasedOn(IList<double> parameterValues, IList<string> variables);
}