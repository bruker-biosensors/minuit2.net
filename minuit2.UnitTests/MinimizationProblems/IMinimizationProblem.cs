using minuit2.net;
using minuit2.net.CostFunctions;

namespace minuit2.UnitTests.MinimizationProblems;

public interface IMinimizationProblem
{ 
    ICostFunction Cost { get; }
    IReadOnlyCollection<ParameterConfiguration> ParameterConfigurations { get; }
    IReadOnlyCollection<double> OptimumParameterValues { get; }
}