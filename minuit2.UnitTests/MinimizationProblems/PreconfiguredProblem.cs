using minuit2.net;
using minuit2.net.CostFunctions;

namespace minuit2.UnitTests.MinimizationProblems;

public record PreconfiguredProblem(
    ICostFunction Cost,
    IReadOnlyCollection<double> OptimumParameterValues,
    IReadOnlyCollection<ParameterConfiguration> ParameterConfigurations)
{
    internal double InitialCostValue()
    {
        var orderedParameterValues = ParameterConfigurations
            .OrderBy(p => Cost.Parameters.IndexOf(p.Name))
            .Select(p => p.Value)
            .ToArray();
        
        return Cost.ValueFor(orderedParameterValues);
    }
}