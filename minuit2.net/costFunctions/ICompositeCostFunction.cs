namespace minuit2.net.costFunctions;

public interface ICompositeCostFunction : ICostFunction
{
    double CompositeValueFor(IList<double> parameterValues);
}