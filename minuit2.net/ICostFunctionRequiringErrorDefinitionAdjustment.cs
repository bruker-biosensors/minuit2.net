namespace minuit2.net;

public interface ICostFunctionRequiringErrorDefinitionAdjustment : ICostFunction
{
    bool RequiresErrorDefinitionAutoScaling { get; }
    ICostFunctionRequiringErrorDefinitionAdjustment WithAutoScaledErrorDefinitionBasedOn(IList<double> parameterValues, IList<string> variables);
}