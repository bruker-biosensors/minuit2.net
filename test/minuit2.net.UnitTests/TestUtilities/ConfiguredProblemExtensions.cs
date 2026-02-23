using ConstrainedNonDeterminism;
using ExampleProblems;
using minuit2.net.CostFunctions;
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

    public static IConfiguredProblem SumWith(this IConfiguredProblem problem, IConfiguredProblem otherProblem)
    {
        var parameterConfigurations = problem.ParameterConfigurations.Concat(otherProblem.ParameterConfigurations);
        var optimumParameterValues = problem.OptimumParameterValues.Concat(otherProblem.OptimumParameterValues);

        List<(ParameterConfiguration Config, double Optimum)> parameters = [];
        foreach (var (config, optimum) in parameterConfigurations.Zip(optimumParameterValues))
        {
            if (parameters.All(p => p.Config.Name != config.Name))
                parameters.Add((config, optimum));
            
            else if (Math.Abs(parameters.Single(p => p.Config.Name == config.Name).Optimum - optimum) > 1E-10)
                throw new ArgumentException($"Parameter {config.Name} has different optimum values in both problems.");
        }

        return new ConfiguredProblem(
            CostFunction.Sum(problem.Cost, otherProblem.Cost),
            parameters.Select(p => p.Optimum).ToList(),
            parameters.Select(p => p.Config).ToList()
        );
    }
}