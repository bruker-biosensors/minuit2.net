namespace minuit2.net.CostFunctions;

public interface ICompositeCostFunction : ICostFunction
{
    double CompositeValueFor(IReadOnlyList<double> parameterValues);
}