using minuit2.net;
using minuit2.net.CostFunctions;

namespace minuit2.UnitTests.MinimizationProblems;

public interface IConfiguredProblem
{
    ICostFunction Cost { get; }
    IReadOnlyCollection<double> OptimumParameterValues { get; }
    IReadOnlyCollection<ParameterConfiguration> ParameterConfigurations { get; }
}