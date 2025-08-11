namespace minuit2.net;

public interface ICompositeCostFunction : ICostFunction
{
    double CompositeValueFor(IList<double> parameterValues);
}