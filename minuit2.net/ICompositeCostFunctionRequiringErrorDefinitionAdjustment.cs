namespace minuit2.net;

public interface ICompositeCostFunctionRequiringErrorDefinitionAdjustment : ICostFunctionRequiringErrorDefinitionAdjustment
{
    double CompositeValueFor(IList<double> parameterValues);
}