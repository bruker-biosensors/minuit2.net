namespace minuit2.net;

public interface ICostFunction
{
    double ValueFor(IList<double> parameters);
}
