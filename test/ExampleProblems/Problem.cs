using minuit2.net;
using minuit2.net.CostFunctions;

namespace ExampleProblems;

public record Problem(
    ICostFunction Cost,
    IReadOnlyCollection<double> OptimumParameterValues,
    IReadOnlyCollection<ParameterConfiguration> ParameterConfigurations) 
    : IProblem
{
    public static Problem Sum(params IProblem[] problems)
    {
        
        var parameterConfigurations = problems.Aggregate(Enumerable.Empty<ParameterConfiguration>(), (acc, next) => acc.Concat(next.ParameterConfigurations));
        var optimumParameterValues = problems.Aggregate(Enumerable.Empty<double>(), (acc, next) => acc.Concat(next.OptimumParameterValues));

        List<(ParameterConfiguration Config, double Optimum)> parameters = [];
        foreach (var (config, optimum) in parameterConfigurations.Zip(optimumParameterValues))
        { 
            if (parameters.All(p => p.Config.Name != config.Name)) 
                parameters.Add((config, optimum));
            
            else if (Math.Abs(parameters.Single(p => p.Config.Name == config.Name).Optimum - optimum) > 1E-10) 
                throw new ArgumentException($"Parameter {config.Name} has different optimum values in both problems.");
        }

        return new Problem(
            CostFunction.Sum(problems.Select(p => p.Cost).ToArray()),
            parameters.Select(p => p.Optimum).ToList(),
            parameters.Select(p => p.Config).ToList());
    }
}