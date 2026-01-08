using minuit2.net;
using minuit2.net.CostFunctions;

namespace ExampleProblems;

internal static class CostFunctionExtensions
{
    public static double ValueFor(this ICostFunction costFunction, IReadOnlyCollection<ParameterConfiguration> parameters)
    {
        var orderedParameterValues = costFunction.Parameters.Select(Value);
        return costFunction.ValueFor(orderedParameterValues);

        double Value(string parameterName) => parameters.Single(config => config.Name == parameterName).Value;
    }

    private static double ValueFor(this ICostFunction costFunction, IEnumerable<double> parameterValues) =>
        costFunction.ValueFor(parameterValues.ToArray());
}