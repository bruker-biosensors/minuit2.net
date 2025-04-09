namespace minuit2.net;

public interface ICostFunction
{
    IList<string> Parameters { get; }
    double ValueFor(IList<double> parameterValues);
    IList<double> GradientFor(IList<double> parameters);
    bool HasGradient { get; }
    double Up { get; }
}
