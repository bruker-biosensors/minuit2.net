namespace ExampleProblems;

public static class ConfiguredProblemExtensions
{
    public static double InitialCostValue(this IConfiguredProblem problem) =>
        problem.Cost.ValueFor(problem.ParameterConfigurations);
}