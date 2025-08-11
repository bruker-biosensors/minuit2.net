namespace minuit2.net;

public interface IComposite
{
    double CompositeValueFor(IList<double> parameterValues);
}