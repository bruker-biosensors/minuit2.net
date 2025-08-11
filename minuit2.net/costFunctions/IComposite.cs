namespace minuit2.net.costFunctions;

public interface IComposite
{
    double CompositeValueFor(IList<double> parameterValues);
}