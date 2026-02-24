namespace ExampleProblems;

public static class ProblemExtensions
{
    public static double InitialCostValue(this IProblem problem) =>
        problem.Cost.ValueFor(problem.ParameterConfigurations);
}