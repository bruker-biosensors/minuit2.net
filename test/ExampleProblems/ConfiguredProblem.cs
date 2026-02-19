using minuit2.net;
using minuit2.net.CostFunctions;

namespace ExampleProblems;

public record ConfiguredProblem(
    ICostFunction Cost,
    IReadOnlyCollection<double> OptimumParameterValues,
    IReadOnlyCollection<ParameterConfiguration> ParameterConfigurations) : IConfiguredProblem;