namespace minuit2.net;

public interface ICostFunction
{
    IList<string> Parameters { get; }
    double ValueFor(IList<double> parameterValues);
    
    IList<double> GradientFor(IList<double> parameterValues);
    bool HasGradient { get; }
    
    double ErrorDefinition { get; }
    bool RequiresErrorDefinitionAutoScaling { get; }
    void AutoScaleErrorDefinitionBasedOn(IList<double> parameterValues, IList<string> variables);
}