using ConstrainedNonDeterminism;
using ExampleProblems;
using static minuit2.net.ParameterConfiguration;

namespace minuit2.net.UnitTests.TestUtilities;

internal static class ConfiguredProblemExtensions
{
    public static IConfiguredProblem WithVariablesAnywhereCloseToOptimumValues(this IConfiguredProblem problem)
    {
        const double maximumRelativeBias = 0.1;
        var newParameterConfigurations = problem.ParameterConfigurations.Zip(problem.OptimumParameterValues, 
            (p, optimumValue) => p.IsFixed 
                ? p 
                : Variable(p.Name, AnyValueCloseTo(optimumValue, maximumRelativeBias), p.LowerLimit, p.UpperLimit))
            .ToArray();
        
        return new ConfiguredProblem(problem.Cost, problem.OptimumParameterValues, newParameterConfigurations);
    }
    
    private static double AnyValueCloseTo(double value, double maximumRelativeBias)
    {
        var maximumBias = Math.Abs(value * maximumRelativeBias);
        return Any.Double().Between(value - maximumBias, value + maximumBias);
    }
}