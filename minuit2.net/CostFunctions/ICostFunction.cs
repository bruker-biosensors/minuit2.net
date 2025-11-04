namespace minuit2.net.CostFunctions;

public interface ICostFunction
{
    IReadOnlyList<string> Parameters { get; }
    bool HasGradient { get; }
    double ErrorDefinition { get; }
    
    double ValueFor(IReadOnlyList<double> parameterValues);
    IReadOnlyList<double> GradientFor(IReadOnlyList<double> parameterValues);

    ICostFunction WithErrorDefinitionRecalculatedBasedOnValid(IMinimizationResult result);
}