namespace minuit2.net;

public interface ICostFunction
{
    IList<string> Parameters { get; }
    double ValueFor(IList<double> parameterValues);
}
