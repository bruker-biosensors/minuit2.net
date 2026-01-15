using ExampleProblems;
using minuit2.net.Minimizers;

namespace minuit2.net.Benchmarks;

internal static class ConfiguredProblemExtensions
{
    public static IMinimizationResult MinimizeWithMigrad(this IConfiguredProblem problem, Strategy strategy)
    {
        var minimizerConfiguration = new MinimizerConfiguration(strategy);
        return Minimizer.Migrad.Minimize(problem.Cost, problem.ParameterConfigurations, minimizerConfiguration);
    }
}