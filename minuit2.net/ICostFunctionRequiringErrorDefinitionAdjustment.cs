namespace minuit2.net;

public interface ICostFunctionRequiringErrorDefinitionAdjustment : ICostFunction
{
    ICostFunctionRequiringErrorDefinitionAdjustment WithAutoScaledErrorDefinitionBasedOn(IList<double> parameterValues, IList<string> variables);
}