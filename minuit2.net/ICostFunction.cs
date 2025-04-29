namespace minuit2.net;

public interface ICostFunction
{
    IList<string> Parameters { get; }
    double ValueFor(IList<double> parameterValues);
    IList<double> GradientFor(IList<double> parameterValues);
    bool HasGradient { get; }
    double ErrorDefinition { get; }
    MinimizationResult Adjusted(MinimizationResult minimizationResult);
}
