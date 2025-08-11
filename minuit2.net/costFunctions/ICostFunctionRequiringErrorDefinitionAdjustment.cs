namespace minuit2.net.costFunctions;

public interface ICostFunctionRequiringErrorDefinitionAdjustment : ICostFunction
{
    ICostFunctionRequiringErrorDefinitionAdjustment WithAutoScaledErrorDefinitionBasedOn(IList<double> parameterValues, IList<string> variables);
}