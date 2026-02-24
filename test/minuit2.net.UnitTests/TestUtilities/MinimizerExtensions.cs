using ExampleProblems;
using minuit2.net.Minimizers;

namespace minuit2.net.UnitTests.TestUtilities;

internal static class MinimizerExtensions
{
    public static IMinimizationResult Minimize(
        this IMinimizer minimizer, 
        IConfiguredProblem problem, 
        MinimizerConfiguration? minimizerConfiguration = null)
    {
        return minimizer.Minimize(problem.Cost, problem.ParameterConfigurations, minimizerConfiguration);
    }
    
    public static IMinimizationResult MinimizeAndRefineErrors(
        this IMinimizer minimizer, 
        IConfiguredProblem problem, 
        MinimizerConfiguration? minimizerConfiguration = null)
    {
        var result = minimizer.Minimize(problem, minimizerConfiguration);
        var adjustedCost = problem.Cost.WithErrorDefinitionRecalculatedBasedOnValid(result);
        return HesseErrorCalculator.Refine(result, adjustedCost);
    }
}