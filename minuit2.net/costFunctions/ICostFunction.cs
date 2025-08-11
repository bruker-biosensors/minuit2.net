namespace minuit2.net.costFunctions;

public interface ICostFunction
{
    IList<string> Parameters { get; }
    bool HasGradient { get; }
    double ErrorDefinition { get; }
    
    double ValueFor(IList<double> parameterValues);
    IList<double> GradientFor(IList<double> parameterValues);
}