using minuit2.net;
using minuit2.net.CostFunctions;

namespace ExampleProblems;

public interface IProblem
{
    ICostFunction Cost { get; }
    IReadOnlyCollection<double> OptimumParameterValues { get; }
    IReadOnlyCollection<ParameterConfiguration> ParameterConfigurations { get; }
}