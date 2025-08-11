namespace minuit2.net.costFunctions;

public interface ICompositeCostFunction
{
    double CompositeValueFor(IList<double> parameterValues);
}