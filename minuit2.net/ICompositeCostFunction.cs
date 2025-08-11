namespace minuit2.net;

public interface ICompositeCostFunction : ICostFunctionRequiringErrorDefinitionAdjustment
{
    double CompositeValueFor(IList<double> parameterValues);
}