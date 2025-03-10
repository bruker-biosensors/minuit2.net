namespace minuit2.net;

public interface ICostFunction
{
    internal IList<string> Parameters { get; }
    double ValueFor(IList<double> parameters);
}
