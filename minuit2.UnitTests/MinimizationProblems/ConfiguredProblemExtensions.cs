using minuit2.UnitTests.TestUtilities;

namespace minuit2.UnitTests.MinimizationProblems;

internal static class ConfiguredProblemExtensions
{
    public static double InitialCostValue(this IConfiguredProblem problem) =>
        problem.Cost.ValueFor(problem.ParameterConfigurations);
}