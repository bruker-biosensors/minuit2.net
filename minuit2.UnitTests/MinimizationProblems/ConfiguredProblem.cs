using minuit2.net;
using minuit2.net.CostFunctions;
using minuit2.UnitTests.TestUtilities;

namespace minuit2.UnitTests.MinimizationProblems;

public record ConfiguredProblem(
    ICostFunction Cost,
    IReadOnlyCollection<double> OptimumParameterValues,
    IReadOnlyCollection<ParameterConfiguration> ParameterConfigurations)
{
    internal double InitialCostValue() => Cost.ValueFor(ParameterConfigurations);
}