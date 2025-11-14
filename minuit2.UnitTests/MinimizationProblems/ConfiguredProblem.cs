using minuit2.net;
using minuit2.net.CostFunctions;

namespace minuit2.UnitTests.MinimizationProblems;

internal record ConfiguredProblem(
    ICostFunction Cost,
    IReadOnlyCollection<double> OptimumParameterValues,
    IReadOnlyCollection<ParameterConfiguration> ParameterConfigurations) : IConfiguredProblem;